using LibrarySystem.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
/// Represents a library member who can borrow items.
/// Has a unique auto-generated library card number (BRW-0001 format).
{
    public class Borrower
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^BRW-\d{4}$")]
        public string LibraryCard { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        public BorrowerStatus Status { get; set; } = BorrowerStatus.Active;

        /// Generates the next sequential library card number for a new borrower.
        /// Queries the highest existing card number and increments.
        /// Format: BRW-0001, BRW-0002, etc.
        public static async Task<string> GenerateLibraryCard(ApplicationDbContext context)
        {
            var maxCard = await context.Borrowers
                .Where(b => b.LibraryCard != null)
                .OrderByDescending(b => b.LibraryCard)
                .Select(b => b.LibraryCard)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (maxCard != null && maxCard.Length == 8)
            {
                int.TryParse(maxCard.Substring(4), out int current);
                nextNumber = current + 1;
            }

            return $"BRW-{nextNumber:D4}";
        }
    }
}