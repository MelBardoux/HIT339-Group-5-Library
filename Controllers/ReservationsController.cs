using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Reception,Manager")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public ReservationsController(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        // GET: Reservations/Reserve
        [Authorize(Roles = "Reception")]
        public async Task<IActionResult> Reserve(string libraryCode, string libraryCard)
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.LibraryCode == libraryCode);

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == libraryCard);

            if (item == null || borrower == null) return NotFound();

            var existingCount = await _context.Reservations
                .CountAsync(r => r.ItemId == item.Id &&
                                 (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready));

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

        // POST: Reservations/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reception")]
        public async Task<IActionResult> Reserve(LoanReserveViewModel viewModel)
        {
            var item = await _context.Items
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.LibraryCode == viewModel.ItemLibraryCode);

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.LibraryCard == viewModel.BorrowerLibraryCard);

            if (item == null || borrower == null) return NotFound();

            var alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.ItemId == item.Id &&
                               r.BorrowerId == borrower.Id &&
                               (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready));

            if (alreadyReserved)
            {
                TempData["ErrorMessage"] = $"{borrower.Name} already has an active reservation for {item.Name}.";
                return RedirectToAction("Borrow", "Loans");
            }

            var nextPosition = await _context.Reservations
                .CountAsync(r => r.ItemId == item.Id &&
                                 (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.Ready)) + 1;

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var staffBranch = await _context.StaffBranches.FirstOrDefaultAsync(sb => sb.UserId == userId);

            var reservation = new Reservation
            {
                ItemId = item.Id,
                BorrowerId = borrower.Id,
                BranchId = staffBranch?.BranchId ?? item.BranchId,
                QueuePosition = nextPosition,
                PlacedAt = DateTime.Now
            };

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

            return RedirectToAction("Borrow", "Loans");
        }

        // GET: Reservations/History
        [Authorize(Roles = "Reception,Manager")]
        public async Task<IActionResult> History(string? status)
        {
            var query = _context.Reservations
    .Include(r => r.Item)
    .Include(r => r.Borrower)
    .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                var parsed = Enum.Parse<ReservationStatus>(status);
                query = query.Where(r => r.Status == parsed);
            }

            var reservations = await query
                .OrderByDescending(r => r.PlacedAt)
                .Select(r => new ReservationHistoryViewModel
                {
                    Id = r.Id,
                    ItemLibraryCode = r.Item.LibraryCode,
                    ItemName = r.Item.Name,
                    BorrowerName = r.Borrower.Name,
                    BorrowerLibraryCard = r.Borrower.LibraryCard,
                    PlacedAt = r.PlacedAt,
                    NotifiedAt = r.NotifiedAt,
                    ExpiresAt = r.ExpiresAt,
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            ViewBag.Status = status;
            return View(reservations);
        }
    }
}