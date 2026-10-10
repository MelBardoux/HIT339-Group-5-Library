using LibrarySystem.Models;

namespace LibrarySystem.Services
{
    /// Simulated multi-channel (Email + SMS) notification service.
    /// Inject this into any controller that needs to notify a borrower.
    public interface INotificationService
    {
        /// Sends one message on both Email and SMS channels. Call SaveChangesAsync afterwards
        /// (or let the calling controller's SaveChangesAsync save it).
        void Send(Borrower borrower, NotificationType type, string subject, string message,
                  Item? item = null, int? loanId = null);

        void NotifyItemBorrowed(Loan loan);

        void NotifyFineCharged(Loan loan);

        /// For the reservation/waitlist feature: tell the next borrower an item is ready.
        void NotifyItemAvailable(Borrower borrower, Item item);

        /// Scans all active loans and sends "due soon" and "overdue / fine accruing" reminders.
        /// Each loan gets at most one reminder of each type per day. Returns how many were sent.
        Task<int> RunDueDateCheckAsync();

        /// Checks for expired reservations (past the 48-hour pickup window).
        /// Expires them, moves up the next borrower in the queue, and notifies them.
        Task<int> RunReservationExpiryCheckAsync();
    }
}
