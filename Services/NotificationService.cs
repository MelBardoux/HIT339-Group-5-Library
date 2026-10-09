using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services
{
    public class NotificationService : INotificationService
    {
        // Send a "due soon" reminder when a loan is this many days (or fewer) from its due date
        public const int DueSoonDays = 2;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Send(Borrower borrower, NotificationType type, string subject, string message,
                         Item? item = null, int? loanId = null)
        {
            var channels = new[]
            {
                (Channel: NotificationChannel.Email, Recipient: borrower.Email),
                (Channel: NotificationChannel.SMS, Recipient: borrower.Phone)
            };

            foreach (var (channel, recipient) in channels)
            {
                var notification = new Notification
                {
                    BorrowerId = borrower.Id,
                    LoanId = loanId,
                    ItemLibraryCode = item?.LibraryCode,
                    ItemName = item?.Name,
                    Type = type,
                    Channel = channel,
                    Recipient = recipient,
                    Subject = subject,
                    Message = message,
                    SentAt = DateTime.Now
                };

                _context.Notifications.Add(notification);

                // Console simulator - shows up in the Visual Studio Output / console window
                _logger.LogInformation("[SIMULATED {Channel}] To: {Recipient} | {Subject} | {Message}",
                    channel, recipient, subject, message);
            }
        }

        public void NotifyItemBorrowed(Loan loan)
        {
            Send(loan.Borrower, NotificationType.ItemBorrowed,
                "Item borrowed",
                $"Hi {loan.Borrower.Name}, you borrowed {loan.Item.Name} ({loan.Item.LibraryCode}). " +
                $"Please return it by {loan.DueDate:dd/MM/yyyy}.",
                loan.Item, loan.Id);
        }

        public void NotifyFineCharged(Loan loan)
        {
            Send(loan.Borrower, NotificationType.FineCharged,
                "Late return fine",
                $"Hi {loan.Borrower.Name}, {loan.Item.Name} ({loan.Item.LibraryCode}) was returned late. " +
                $"A fine of {loan.Fine:C} has been added to your account. Your account is suspended until it is paid.",
                loan.Item, loan.Id);
        }

        public void NotifyItemAvailable(Borrower borrower, Item item)
        {
            Send(borrower, NotificationType.ItemAvailable,
                "Your reserved item is ready",
                $"Hi {borrower.Name}, {item.Name} ({item.LibraryCode}) is now available and is being held for you.",
                item);
        }

        public async Task<int> RunDueDateCheckAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var startOfToday = DateTime.Today;
            int sent = 0;

            var activeLoans = await _context.Loans
                .Include(l => l.Item)
                .Include(l => l.Borrower)
                .Where(l => l.ReturnedDate == null)
                .ToListAsync();

            foreach (var loan in activeLoans)
            {
                int daysLeft = loan.DueDate.DayNumber - today.DayNumber;

                NotificationType? type = null;
                string subject = "", message = "";

                if (daysLeft < 0)
                {
                    var fineSoFar = Loan.CalculateFine(loan.DueDate, today);
                    type = NotificationType.Overdue;
                    subject = "Item overdue - fine accruing";
                    message = $"Hi {loan.Borrower.Name}, {loan.Item.Name} ({loan.Item.LibraryCode}) was due on " +
                              $"{loan.DueDate:dd/MM/yyyy} and is {-daysLeft} day(s) overdue. " +
                              $"Your fine so far is {fineSoFar:C} and grows by $1 each day.";
                }
                else if (daysLeft <= DueSoonDays)
                {
                    type = NotificationType.DueSoon;
                    subject = "Item due soon";
                    message = $"Hi {loan.Borrower.Name}, {loan.Item.Name} ({loan.Item.LibraryCode}) is due " +
                              (daysLeft == 0 ? "today" : $"in {daysLeft} day(s)") +
                              $" ({loan.DueDate:dd/MM/yyyy}).";
                }

                if (type == null) continue;

                // Don't send the same reminder for the same loan twice in one day
                bool alreadySentToday = await _context.Notifications.AnyAsync(n =>
                    n.LoanId == loan.Id && n.Type == type && n.SentAt >= startOfToday);

                if (alreadySentToday) continue;

                Send(loan.Borrower, type.Value, subject, message, loan.Item, loan.Id);
                sent++;
            }

            await _context.SaveChangesAsync();
            return sent;
        }

        /// Checks for reservations past the 48-hour pickup window.
        /// Expired reservations are marked as such, and the next borrower in the queue is promoted.
        public async Task<int> RunReservationExpiryCheckAsync()
        {
            var now = DateTime.Now;
            int processed = 0;

            // Find all Ready reservations that have passed their expiry time
            var expiredReservations = await _context.Reservations
                .Include(r => r.Item)
                .Include(r => r.Borrower)
                .Where(r => r.Status == ReservationStatus.Ready && r.ExpiresAt != null && r.ExpiresAt <= now)
                .ToListAsync();

            foreach (var expired in expiredReservations)
            {
                expired.Status = ReservationStatus.Expired;
                expired.Item.ReservedForBorrowerId = null;

                // Find the next waiting borrower in the queue for this item
                var next = await _context.Reservations
                    .Include(r => r.Borrower)
                    .Where(r => r.ItemId == expired.ItemId && r.Status == ReservationStatus.Waiting)
                    .OrderBy(r => r.QueuePosition)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    next.Status = ReservationStatus.Ready;
                    next.NotifiedAt = now;
                    next.ExpiresAt = now.AddHours(48);
                    expired.Item.Status = ItemStatus.Reserved;
                    expired.Item.ReservedForBorrowerId = next.BorrowerId;
                    NotifyItemAvailable(next.Borrower, expired.Item);
                }
                else
                {
                    // No one else waiting, item becomes available
                    expired.Item.Status = ItemStatus.Available;
                }

                processed++;
            }

            if (processed > 0)
                await _context.SaveChangesAsync();

            return processed;
        }
    }
}
