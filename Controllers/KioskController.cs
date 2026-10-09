using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    /// Public self-service kiosk for in-library touch screens.
    /// Patrons scan (or type) their library card, then scan item codes to check out
    /// or view a summary of their account. No staff login is needed.
    /// Barcode scanners act like a keyboard that types the code and presses Enter,
    /// so the scan boxes work with both a scanner and the on-screen keyboard.
    [AllowAnonymous]
    public class KioskController : Controller
    {
        public const int MaxActiveLoans = 10;
        public const int LoanDays = 14;

        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public KioskController(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        // GET: /Kiosk  - welcome screen, scan library card
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Kiosk  - card scanned
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string libraryCard)
        {
            var card = (libraryCard ?? "").Trim().ToUpper();
            var borrower = await _context.Borrowers.FirstOrDefaultAsync(b => b.LibraryCard == card);

            if (borrower == null)
            {
                TempData["KioskError"] = "Library card not recognised. Please try again or see the front desk.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Menu), new { card = borrower.LibraryCard });
        }

        // GET: /Kiosk/Menu?card=BRW-0001
        public async Task<IActionResult> Menu(string card)
        {
            var borrower = await GetBorrowerSummaryAsync(card);
            if (borrower == null) return RedirectToAction(nameof(Index));
            return View(borrower);
        }

        // GET: /Kiosk/Checkout?card=BRW-0001
        public async Task<IActionResult> Checkout(string card)
        {
            var borrower = await GetBorrowerSummaryAsync(card);
            if (borrower == null) return RedirectToAction(nameof(Index));

            var today = DateOnly.FromDateTime(DateTime.Now);

            var viewModel = new KioskCheckoutViewModel
            {
                Borrower = borrower,
                CheckedOutToday = await LoanRows(_context.Loans
                        .Where(l => l.Borrower.LibraryCard == borrower.LibraryCard
                                 && l.BorrowedDate == today
                                 && l.ReturnedDate == null))
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Kiosk/Checkout  - item scanned
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string card, string itemCode)
        {
            var borrower = await _context.Borrowers.FirstOrDefaultAsync(b => b.LibraryCard == card);
            if (borrower == null) return RedirectToAction(nameof(Index));

            var summary = await GetBorrowerSummaryAsync(card);
            if (summary == null || !summary.CanBorrow)
            {
                TempData["KioskError"] = "Your account has a suspension or unpaid fine. Please see the front desk.";
                return RedirectToAction(nameof(Checkout), new { card });
            }

            if (summary.ActiveLoanCount >= MaxActiveLoans)
            {
                TempData["KioskError"] = $"You already have {MaxActiveLoans} items on loan, which is the limit.";
                return RedirectToAction(nameof(Checkout), new { card });
            }

            var code = (itemCode ?? "").Trim().ToUpper();
            var item = await _context.Items.FirstOrDefaultAsync(i => i.LibraryCode == code);

            if (item == null)
            {
                TempData["KioskError"] = $"Item code \"{code}\" was not found. Please scan again.";
                return RedirectToAction(nameof(Checkout), new { card });
            }

            bool reservedForMe = item.Status == ItemStatus.Reserved && item.ReservedForBorrowerId == borrower.Id;

            if (item.Status != ItemStatus.Available && !reservedForMe)
            {
                TempData["KioskError"] = $"{item.Name} can't be borrowed right now (status: {item.Status}). " +
                                         "Please see the front desk.";
                return RedirectToAction(nameof(Checkout), new { card });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var loan = new Loan
            {
                Item = item,
                ItemId = item.Id,
                Borrower = borrower,
                BorrowerId = borrower.Id,
                BorrowedDate = today,
                DueDate = today.AddDays(LoanDays)
            };

            if (reservedForMe) item.ReservedForBorrowerId = null;
            item.Status = ItemStatus.Borrowed;
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();   // save first so the loan has an Id

            _notifications.NotifyItemBorrowed(loan);
            await _context.SaveChangesAsync();

            TempData["KioskSuccess"] = $"✓ {item.Name} checked out. Due back {loan.DueDate:dddd d MMMM}.";
            return RedirectToAction(nameof(Checkout), new { card });
        }

        // GET: /Kiosk/Account?card=BRW-0001
        public async Task<IActionResult> Account(string card)
        {
            var borrower = await GetBorrowerSummaryAsync(card);
            if (borrower == null) return RedirectToAction(nameof(Index));

            var borrowerLoans = _context.Loans.Where(l => l.Borrower.LibraryCard == borrower.LibraryCard);

            var viewModel = new KioskAccountViewModel
            {
                Borrower = borrower,
                CurrentLoans = await LoanRows(borrowerLoans.Where(l => l.ReturnedDate == null))
                    .OrderBy(l => l.DueDate).ToListAsync(),
                UnpaidFines = await LoanRows(borrowerLoans.Where(l => l.Fine > 0)).ToListAsync(),
                ReservedItems = await _context.Items
                    .Where(i => i.ReservedForBorrower != null && i.ReservedForBorrower.LibraryCard == borrower.LibraryCard)
                    .Select(i => i.Name + " (" + i.LibraryCode + ") - " +
                                 (i.Status == ItemStatus.Reserved ? "ready to collect" : "waiting"))
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // ---------- helpers ----------

        private async Task<KioskBorrowerViewModel?> GetBorrowerSummaryAsync(string? card)
        {
            if (string.IsNullOrWhiteSpace(card)) return null;

            var borrower = await _context.Borrowers.FirstOrDefaultAsync(b => b.LibraryCard == card);
            if (borrower == null) return null;

            return new KioskBorrowerViewModel
            {
                LibraryCard = borrower.LibraryCard,
                Name = borrower.Name,
                IsSuspended = borrower.Status == BorrowerStatus.Suspended,
                OutstandingFines = await _context.Loans
                    .Where(l => l.BorrowerId == borrower.Id && l.Fine > 0)
                    .SumAsync(l => l.Fine ?? 0),
                ActiveLoanCount = await _context.Loans
                    .CountAsync(l => l.BorrowerId == borrower.Id && l.ReturnedDate == null)
            };
        }

        private static IQueryable<KioskLoanRow> LoanRows(IQueryable<Loan> loans)
        {
            return loans.Select(l => new KioskLoanRow
            {
                LibraryCode = l.Item.LibraryCode,
                ItemName = l.Item.Name,
                ItemType = l.Item is Book ? "Book" : l.Item is Toy ? "Toy" : "Music",
                BorrowedDate = l.BorrowedDate,
                DueDate = l.DueDate,
                ReturnedDate = l.ReturnedDate,
                Fine = l.Fine
            });
        }
    }
}
