using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Reception")]
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index(string? filter, string? sort)
        {
            var query = _context.Loans
                .Include(l => l.Item)
                .Include(l => l.Borrower)
                .AsQueryable();

            if (filter == "notreturned")
                query = query.Where(l => l.ReturnedDate == null);
            else if (filter == "returned")
                query = query.Where(l => l.ReturnedDate != null);
            else if (filter == "hasfine")
                query = query.Where(l => l.Fine > 0);

            query = sort switch
            {
                "fine_high" => query.OrderByDescending(l => l.Fine ?? 0),
                "fine_low" => query.OrderBy(l => l.Fine ?? 0),
                "overdue" => query.Where(l => l.ReturnedDate == null).OrderBy(l => l.DueDate),
                _ => query.OrderByDescending(l => l.BorrowedDate)
            };

            var loans = await query
                .Select(l => new LoanIndexViewModel
                {
                    Id = l.Id,
                    ItemLibraryCode = l.Item.LibraryCode,
                    ItemName = l.Item.Name,
                    BorrowerName = l.Borrower.Name,
                    BorrowerLibraryCard = l.Borrower.LibraryCard,
                    BorrowedDate = l.BorrowedDate,
                    DueDate = l.DueDate,
                    ReturnedDate = l.ReturnedDate,
                    Fine = l.Fine
                })
                .ToListAsync();

            ViewBag.Filter = filter;
            ViewBag.Sort = sort;

            return View(loans);
        }

        // GET: Loans/Borrow
        public IActionResult Borrow()
        {
            return View(new LoanBorrowViewModel());
        }

        // POST: Loans/Borrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(LoanBorrowViewModel viewModel)
        {
            if (!viewModel.IsConfirmed)
            {
                var borrower = await _context.Borrowers
                    .FirstOrDefaultAsync(b => b.LibraryCard == viewModel.LibraryCard);

                if (borrower == null)
                {
                    viewModel.ErrorMessage = "Borrower not found.";
                    return View(viewModel);
                }

                var outstandingFines = await _context.Loans
                    .Where(l => l.BorrowerId == borrower.Id && l.Fine > 0)
                    .SumAsync(l => l.Fine ?? 0);

                viewModel.BorrowerResult = new BorrowerLookupResult
                {
                    Id = borrower.Id,
                    LibraryCard = borrower.LibraryCard,
                    Name = borrower.Name,
                    Email = borrower.Email,
                    Status = borrower.Status.ToString(),
                    IsSuspended = borrower.Status == BorrowerStatus.Suspended,
                    OutstandingFines = outstandingFines
                };

                viewModel.ItemResults = new List<ItemLookupResult>();

                foreach (var code in viewModel.LibraryCodes ?? new List<string>())
                {
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    var item = await _context.Items
                        .FirstOrDefaultAsync(i => i.LibraryCode == code.Trim().ToUpper());

                    if (item == null)
                    {
                        viewModel.ItemResults.Add(new ItemLookupResult
                        {
                            LibraryCode = code,
                            Name = "Not found",
                            Type = "-",
                            Status = "Not found",
                            IsAvailable = false
                        });
                    }
                    else
                    {
                        viewModel.ItemResults.Add(new ItemLookupResult
                        {
                            Id = item.Id,
                            LibraryCode = item.LibraryCode,
                            Name = item.Name,
                            Type = item.GetType().Name,
                            Status = item.Status.ToString(),
                            IsAvailable = item.Status == ItemStatus.Available ||
                                          (item.Status == ItemStatus.Reserved && item.ReservedForBorrowerId == borrower.Id)
                        });
                    }
                }

                viewModel.IsLookedUp = true;
                return View(viewModel);
            }

            var confirmedBorrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == viewModel.LibraryCard);

            if (confirmedBorrower == null)
            {
                viewModel.ErrorMessage = "Borrower no longer found.";
                return View(viewModel);
            }

            if (confirmedBorrower.Status == BorrowerStatus.Suspended && !viewModel.BorrowerOverride)
            {
                viewModel.ErrorMessage = "Borrower is suspended. Use override to proceed.";
                return View(viewModel);
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var dueDate = today.AddDays(14);

            foreach (var code in viewModel.LibraryCodes ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(code)) continue;

                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.LibraryCode == code.Trim().ToUpper());

                if (item == null) continue;

                if (item.Status == ItemStatus.Reserved && item.ReservedForBorrowerId == confirmedBorrower.Id)
                {
                    item.ReservedForBorrowerId = null;
                }
                else if (item.Status != ItemStatus.Available)
                {
                    continue;
                }

                var loan = new Loan
                {
                    ItemId = item.Id,
                    BorrowerId = confirmedBorrower.Id,
                    BorrowedDate = today,
                    DueDate = dueDate
                };

                item.Status = ItemStatus.Borrowed;
                _context.Loans.Add(loan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Reserve
        public async Task<IActionResult> Reserve(string libraryCode, string libraryCard)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.LibraryCode == libraryCode);

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == libraryCard);

            if (item == null || borrower == null) return NotFound();

            var viewModel = new LoanReserveViewModel
            {
                ItemLibraryCode = item.LibraryCode,
                ItemName = item.Name,
                ItemStatus = item.Status.ToString(),
                BorrowerName = borrower.Name,
                BorrowerLibraryCard = borrower.LibraryCard
            };

            return View(viewModel);
        }

        // POST: Loans/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(LoanReserveViewModel viewModel)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.LibraryCode == viewModel.ItemLibraryCode);

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == viewModel.BorrowerLibraryCard);

            if (item == null || borrower == null) return NotFound();

            item.ReservedForBorrowerId = borrower.Id;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{item.Name} ({item.LibraryCode}) has been reserved for {borrower.Name} ({borrower.LibraryCard}).";
            return RedirectToAction("Borrow");
        }

        // GET: Loans/Return/5
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Item)
                    .ThenInclude(i => i.ReservedForBorrower)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null || loan.ReturnedDate != null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var fine = Loan.CalculateFine(loan.DueDate, today);

            var viewModel = new LoanReturnViewModel
            {
                Id = loan.Id,
                ItemLibraryCode = loan.Item.LibraryCode,
                ItemName = loan.Item.Name,
                BorrowerName = loan.Borrower.Name,
                BorrowerLibraryCard = loan.Borrower.LibraryCard,
                BorrowedDate = loan.BorrowedDate,
                DueDate = loan.DueDate,
                ReturnDate = today,
                Fine = fine,
                ReservedForName = loan.Item.ReservedForBorrower?.Name
            };

            return View(viewModel);
        }

        // POST: Loans/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, LoanReturnViewModel viewModel)
        {
            var loan = await _context.Loans
                .Include(l => l.Item)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null || loan.ReturnedDate != null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var fine = Loan.CalculateFine(loan.DueDate, today);

            loan.ReturnedDate = today;
            loan.Fine = fine > 0 ? fine : null;

            if (loan.Item.ReservedForBorrowerId != null)
            {
                loan.Item.Status = ItemStatus.Reserved;
            }
            else
            {
                loan.Item.Status = ItemStatus.Available;
            }

            if (fine > 0)
            {
                loan.Borrower.Status = BorrowerStatus.Suspended;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/PayFine/5
        public async Task<IActionResult> PayFine(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Item)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null || loan.Fine == null || loan.Fine == 0) return NotFound();

            var viewModel = new LoanPayFineViewModel
            {
                Id = loan.Id,
                ItemLibraryCode = loan.Item.LibraryCode,
                ItemName = loan.Item.Name,
                BorrowerName = loan.Borrower.Name,
                Fine = loan.Fine.Value
            };

            return View(viewModel);
        }

        // POST: Loans/PayFine/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFine(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            loan.Fine = null;

            var hasOtherFines = await _context.Loans
                .AnyAsync(l => l.BorrowerId == loan.BorrowerId && l.Id != loan.Id && l.Fine > 0);

            if (!hasOtherFines)
            {
                loan.Borrower.Status = BorrowerStatus.Active;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}