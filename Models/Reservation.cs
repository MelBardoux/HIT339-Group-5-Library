using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Models
/// Represents a reservation in the waitlist queue for a library item.
/// Supports multiple borrowers waiting for the same item, ordered by queue position.
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

    /// Tracks the lifecycle of a reservation in the waitlist queue.
    public enum ReservationStatus
    {
        Waiting = 0,    // In queue, item not yet available
        Ready = 1,      // Item available, borrower notified
        Collected = 2,  // Borrower collected the item
        Cancelled = 3,  // Borrower cancelled the reservation
        Expired = 4     // Borrower did not collect within the pickup window
    }
}