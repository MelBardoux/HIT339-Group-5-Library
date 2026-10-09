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
    }
}
