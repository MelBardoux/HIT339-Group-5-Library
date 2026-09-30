using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Models
/// Represents a loan transaction linking a borrowed item to a borrower.
/// Tracks borrow date, due date, return date, and any late fine.
{
    public class Loan
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int BorrowerId { get; set; }
        public Borrower Borrower { get; set; }

        public DateOnly BorrowedDate { get; set; }

        public DateOnly DueDate { get; set; }

        public DateOnly? ReturnedDate { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Fine { get; set; }

        /// Calculates the fine based on how many days late an item is returned.
        /// Returns $0 if returned on or before the due date, otherwise $1 per day late.
        public static decimal CalculateFine(DateOnly dueDate, DateOnly returnedDate)
        {
            if (returnedDate.DayNumber <= dueDate.DayNumber)
            {
                return 0;
            }

            int daysLate = returnedDate.DayNumber - dueDate.DayNumber;
            return daysLate * 1.0m;
        }
    }
}