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

            // Check if this borrower has a ready reservation for this item
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ItemId == item.Id
                                       && r.BorrowerId == borrower.Id
                                       && r.Status == ReservationStatus.Ready);

            if (item.Status != ItemStatus.Available && reservation == null)
            {
                TempData["KioskError"] = $"{item.Name} can't be borrowed right now (status: {item.Status}). " +
                                         "Please see the front desk.";
                return RedirectToAction(nameof(Checkout), new { card });
            }

            // Redirect to confirmation page before finalising
            var today = DateOnly.FromDateTime(DateTime.Now);
            var confirm = new KioskConfirmViewModel
            {
                LibraryCard = card,
                ItemCode = item.LibraryCode,
                ItemName = item.Name,
                ItemType = item.GetType().Name,
                DueDate = today.AddDays(LoanDays).ToString("dddd d MMMM yyyy"),
                IsReservation = reservation != null
            };

            TempData["ConfirmData"] = System.Text.Json.JsonSerializer.Serialize(confirm);
            return RedirectToAction(nameof(Confirm), new { card });
        }

        // GET: /Kiosk/Confirm - shows item details before finalising checkout
        public IActionResult Confirm(string card)
        {
            var json = TempData["ConfirmData"] as string;
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(nameof(Checkout), new { card });

            var viewModel = System.Text.Json.JsonSerializer.Deserialize<KioskConfirmViewModel>(json);
            return View(viewModel);
        }

        // POST: /Kiosk/Confirm - finalises the checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(KioskConfirmViewModel viewModel)
        {
            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == viewModel.LibraryCard);
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.LibraryCode == viewModel.ItemCode);

            if (borrower == null || item == null)
                return RedirectToAction(nameof(Index));

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ItemId == item.Id
                                       && r.BorrowerId == borrower.Id
                                       && r.Status == ReservationStatus.Ready);

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

            if (reservation != null)
            {
                reservation.Status = ReservationStatus.Collected;
            }

            item.Status = ItemStatus.Borrowed;
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            _notifications.NotifyItemBorrowed(loan);
            await _context.SaveChangesAsync();

            TempData["KioskSuccess"] = $"{item.Name} checked out. Due back {loan.DueDate:dddd d MMMM}.";
            return RedirectToAction(nameof(Checkout), new { card = viewModel.LibraryCard });
        }

        // GET: /Kiosk/Return?card=BRW-0001
        public async Task<IActionResult> Return(string card)
        {
            var borrower = await GetBorrowerSummaryAsync(card);
            if (borrower == null) return RedirectToAction(nameof(Index));

            var viewModel = new KioskReturnViewModel
            {
                Borrower = borrower
            };

            return View(viewModel);
        }

        // POST: /Kiosk/Return - item scanned for return
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(string card, string itemCode)
        {
            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == card);
            if (borrower == null) return RedirectToAction(nameof(Index));

            var code = (itemCode ?? "").Trim().ToUpper();
            var loan = await _context.Loans
                .Include(l => l.Item).ThenInclude(i => i.Branch)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Item.LibraryCode == code
                                       && l.BorrowerId == borrower.Id
                                       && l.ReturnedDate == null);

            if (loan == null)
            {
                TempData["KioskError"] = $"No active loan found for \"{code}\" on your account.";
                return RedirectToAction(nameof(Return), new { card });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var fine = Loan.CalculateFine(loan.DueDate, today);

            loan.ReturnedDate = today;
            loan.Fine = fine > 0 ? fine : null;

            var hasCrossBranchWarning = false;

            // Check for next reservation
            var nextReservation = await _context.Reservations
                .Include(r => r.Borrower)
                .Where(r => r.ItemId == loan.ItemId && r.Status == ReservationStatus.Waiting)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation != null)
            {
                if (loan.Item.BranchId == nextReservation.BranchId)
                {
                    nextReservation.Status = ReservationStatus.Ready;
                    nextReservation.NotifiedAt = DateTime.Now;
                    nextReservation.ExpiresAt = DateTime.Now.AddHours(48);
                    loan.Item.Status = ItemStatus.Reserved;
                    loan.Item.ReservedForBorrowerId = nextReservation.BorrowerId;
                    _notifications.NotifyItemAvailable(nextReservation.Borrower, loan.Item);
                }
                else
                {
                    // Cross-branch reservation: kiosk cannot create transfers
                    hasCrossBranchWarning = true;
                    loan.Item.Status = ItemStatus.Available;
                    loan.Item.ReservedForBorrowerId = null;
                    TempData["KioskWarning"] = $"{loan.Item.Name} has a reservation at another branch. Please hand this item to staff so they can arrange the transfer.";
                }
            }
            else
            {
                loan.Item.Status = ItemStatus.Available;
                loan.Item.ReservedForBorrowerId = null;
            }

            if (fine > 0)
            {
                loan.Borrower.Status = BorrowerStatus.Suspended;
                _notifications.NotifyFineCharged(loan);
            }

            await _context.SaveChangesAsync();
            if (!hasCrossBranchWarning)
            {
                var result = fine > 0
                    ? $"{loan.Item.Name} returned. A late fee of {fine:C} has been applied."
                    : $"{loan.Item.Name} returned successfully.";
                TempData["KioskSuccess"] = result;
            }
            return RedirectToAction(nameof(Return), new { card });
        }

        // GET: /Kiosk/Receipt?card=BRW-0001
        public async Task<IActionResult> Receipt(string card)
        {
            var borrower = await GetBorrowerSummaryAsync(card);
            if (borrower == null) return RedirectToAction(nameof(Index));

            var today = DateOnly.FromDateTime(DateTime.Now);
            var viewModel = new KioskCheckoutViewModel
            {
                Borrower = borrower,
                CheckedOutToday = await LoanRows(_context.Loans
                        .Where(l => l.Borrower.LibraryCard == card
                                 && l.BorrowedDate == today
                                 && l.ReturnedDate == null))
                    .ToListAsync()
            };

            return View(viewModel);
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
                // Query active reservations from the new Reservations table
                ReservedItems = await _context.Reservations
                    .Include(r => r.Item)
                    .Where(r => r.Borrower.LibraryCard == borrower.LibraryCard
                             && (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready))
                    .OrderBy(r => r.QueuePosition)
                    .Select(r => r.Item.Name + " (" + r.Item.LibraryCode + ") - " +
                                 (r.Status == ReservationStatus.Ready ? "ready to collect" : "waiting, #" + r.QueuePosition + " in queue"))
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
