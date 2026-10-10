using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        // The item being reserved
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        // The borrower requesting the reservation
        public int BorrowerId { get; set; }
        public Borrower Borrower { get; set; } = null!;

        // The branch where the reservation was placed (for cross-branch transfers)
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        // When the reservation was placed
        public DateTime PlacedAt { get; set; } = DateTime.Now;

        // Position in the waitlist queue (1 = next in line)
        public int QueuePosition { get; set; }

        // Current status of this reservation
        public ReservationStatus Status { get; set; } = ReservationStatus.Waiting;

        // When the borrower was notified the item is available
        public DateTime? NotifiedAt { get; set; }

        // Deadline for the borrower to collect (48 hours after notification)
        public DateTime? ExpiresAt { get; set; }
    }

    public enum ReservationStatus
    {
        Waiting = 0,
        Ready = 1,
        Collected = 2,
        Cancelled = 3,
        Expired = 4
    }
}