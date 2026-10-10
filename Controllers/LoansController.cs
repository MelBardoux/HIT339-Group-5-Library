using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services;
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
        private readonly INotificationService _notifications;

        public LoansController(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
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
                        // Check if this borrower has a ready reservation for this item
                        var hasReadyReservation = await _context.Reservations
                            .AnyAsync(r => r.ItemId == item.Id
                                        && r.BorrowerId == borrower.Id
                                        && r.Status == ReservationStatus.Ready);

                        viewModel.ItemResults.Add(new ItemLookupResult
                        {
                            Id = item.Id,
                            LibraryCode = item.LibraryCode,
                            Name = item.Name,
                            Type = item.GetType().Name,
                            Status = item.Status.ToString(),
                            IsAvailable = item.Status == ItemStatus.Available || hasReadyReservation
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
            var newLoans = new List<Loan>();

            foreach (var code in viewModel.LibraryCodes ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(code)) continue;

                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.LibraryCode == code.Trim().ToUpper());

                if (item == null) continue;

                // Mark the reservation as collected if one exists
                var borrowReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.ItemId == item.Id
                                           && r.BorrowerId == confirmedBorrower.Id
                                           && r.Status == ReservationStatus.Ready);
                if (borrowReservation != null)
                {
                    borrowReservation.Status = ReservationStatus.Collected;
                }
                else if (item.Status != ItemStatus.Available && borrowReservation == null)
                {
                    continue;
                }

                var loan = new Loan
                {
                    ItemId = item.Id,
                    Item = item,
                    BorrowerId = confirmedBorrower.Id,
                    Borrower = confirmedBorrower,
                    BorrowedDate = today,
                    DueDate = dueDate
                };

                item.Status = ItemStatus.Borrowed;
                _context.Loans.Add(loan);
                newLoans.Add(loan);
            }

            await _context.SaveChangesAsync();

            // Send a simulated Email + SMS for each item borrowed (loans now have Ids)
            foreach (var loan in newLoans)
            {
                _notifications.NotifyItemBorrowed(loan);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Reserve
        // Displays the reservation confirmation page with the borrower's queue position
        public async Task<IActionResult> Reserve(string libraryCode, string libraryCard)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.LibraryCode == libraryCode);

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == libraryCard);

            if (item == null || borrower == null) return NotFound();

            // Check how many active reservations already exist for this item.
            // Only count Waiting and Ready statuses — Collected, Cancelled, and Expired
            // are no longer in the queue.
            var existingCount = await _context.Reservations
                .CountAsync(r => r.ItemId == item.Id &&
                                 (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready));

            // The new borrower's position is one after the last active reservation.
            // Position 1 means they are next in line when the item becomes available.
            var viewModel = new LoanReserveViewModel
            {
                ItemLibraryCode = item.LibraryCode,
                ItemName = item.Name,
                ItemStatus = item.Status.ToString(),
                BorrowerName = borrower.Name,
                BorrowerLibraryCard = borrower.LibraryCard,
                QueuePosition = existingCount + 1,
                ExistingReservations = existingCount
            };

            return View(viewModel);
        }

        // POST: Loans/Reserve
        // Creates a reservation in the waitlist queue. Notifies immediately if the item is available.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(LoanReserveViewModel viewModel)
        {
            var item = await _context.Items
    .Include(i => i.Branch)
    .FirstOrDefaultAsync(i => i.LibraryCode == viewModel.ItemLibraryCode);

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == viewModel.BorrowerLibraryCard);

            if (item == null || borrower == null) return NotFound();

            // Prevent duplicate reservations — a borrower cannot queue twice for the same item
            var alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.ItemId == item.Id &&
                               r.BorrowerId == borrower.Id &&
                               (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready));

            if (alreadyReserved)
            {
                TempData["ErrorMessage"] = $"{borrower.Name} already has an active reservation for {item.Name}.";
                return RedirectToAction("Borrow");
            }

            // Determine queue position: one after the last active reservation
            var nextPosition = await _context.Reservations
                .CountAsync(r => r.ItemId == item.Id &&
                                 (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready)) + 1;

            var reservation = new Reservation
            {
                ItemId = item.Id,
                BorrowerId = borrower.Id,
                QueuePosition = nextPosition,
                PlacedAt = DateTime.Now
            };

            // If the item is currently available and this is the first in the queue,
            // mark it as ready for pickup immediately
            if (item.Status == ItemStatus.Available && nextPosition == 1)
            {
                reservation.Status = ReservationStatus.Ready;
                reservation.NotifiedAt = DateTime.Now;
                reservation.ExpiresAt = DateTime.Now.AddHours(48);
                item.Status = ItemStatus.Reserved;
                item.ReservedForBorrowerId = borrower.Id;
                _notifications.NotifyItemAvailable(borrower, item);
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = nextPosition == 1 && item.Status == ItemStatus.Reserved
                ? $"{item.Name} ({item.LibraryCode}) is ready for {borrower.Name} to collect. They have 48 hours."
                : $"{borrower.Name} ({borrower.LibraryCard}) is #{nextPosition} in the queue for {item.Name} ({item.LibraryCode}).";

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
        // Processes a return, calculates fines, and promotes the next borrower in the waitlist queue.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, LoanReturnViewModel viewModel)
        {
            var loan = await _context.Loans
    .Include(l => l.Item).ThenInclude(i => i.Branch)
    .Include(l => l.Borrower)
    .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null || loan.ReturnedDate != null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var fine = Loan.CalculateFine(loan.DueDate, today);

            loan.ReturnedDate = today;
            loan.Fine = fine > 0 ? fine : null;

            // If the borrower who just returned the item had a fulfilled reservation,
            // mark it as Collected
            var fulfilledReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ItemId == loan.ItemId &&
                                          r.BorrowerId == loan.BorrowerId &&
                                          r.Status == ReservationStatus.Ready);
            if (fulfilledReservation != null)
            {
                fulfilledReservation.Status = ReservationStatus.Collected;
            }

            // Check if the next borrower in the waitlist queue is waiting
            var nextReservation = await _context.Reservations
                .Include(r => r.Borrower)
                .Where(r => r.ItemId == loan.ItemId && r.Status == ReservationStatus.Waiting)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation != null)
            {
                // Notify the next borrower and give them 48 hours to collect
                nextReservation.Status = ReservationStatus.Ready;
                nextReservation.NotifiedAt = DateTime.Now;
                nextReservation.ExpiresAt = DateTime.Now.AddHours(48);
                loan.Item.Status = ItemStatus.Reserved;
                loan.Item.ReservedForBorrowerId = nextReservation.BorrowerId;
                _notifications.NotifyItemAvailable(nextReservation.Borrower, loan.Item);
            }
            else
            {
                // No one waiting — item is available again
                loan.Item.Status = ItemStatus.Available;
                loan.Item.ReservedForBorrowerId = null;
            }

            if (fine > 0)
            {
                loan.Borrower.Status = BorrowerStatus.Suspended;
                _notifications.NotifyFineCharged(loan);
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