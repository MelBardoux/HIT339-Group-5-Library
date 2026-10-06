using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    /// A simulated email or SMS message sent to a borrower.
    /// Nothing is actually sent - each message is stored here and written to the console log,
    /// so staff can see it on the Notifications dashboard.
    /// The item code and name are copied in (a snapshot) so the history stays correct
    /// even if the item is later edited or deleted.
    public class Notification
    {
        public int Id { get; set; }

        public int BorrowerId { get; set; }
        public Borrower Borrower { get; set; } = null!;

        // Optional link back to the loan that triggered it (used to avoid duplicate reminders)
        public int? LoanId { get; set; }

        [StringLength(20)]
        public string? ItemLibraryCode { get; set; }

        [StringLength(100)]
        public string? ItemName { get; set; }

        public NotificationType Type { get; set; }

        public NotificationChannel Channel { get; set; }

        /// Email address or phone number the message was "sent" to.
        [Required]
        [StringLength(250)]
        public string Recipient { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
