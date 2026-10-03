using LibrarySystem.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
// Abstract base class for all library items using TPH (Table Per Hierarchy) inheritance.
//Book, Toy, and Music inherit from this class and share the Items database table.

{
    public abstract class Item
    {
        public int Id { get; set; }

        // Added Branch properties to item class
        public int BranchId { get; set; } = 1;
        public Branch Branch { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s.\-']+$",
            ErrorMessage = "Name may only contain letters, spaces, and . - '")]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{3}-[0-9]{4}$",
            ErrorMessage = "Library code must be three uppercase letters, a hyphen, then four digits (e.g. TOY-0042).")]
        public string LibraryCode { get; set; }

        public ItemStatus Status { get; set; }

        public DateOnly DateAdded { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public bool OnOrder { get; set; }

        public int? ReservedForBorrowerId { get; set; }
        public Borrower? ReservedForBorrower { get; set; }

        public static async Task<string> GenerateLibraryCode<T>(ApplicationDbContext context, string prefix) where T : Item
        {
            var maxCode = await context.Items
                .OfType<T>()
                .Where(i => i.LibraryCode != null)
                .OrderByDescending(i => i.LibraryCode)
                .Select(i => i.LibraryCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (maxCode != null && maxCode.Length == 8)
            {
                int.TryParse(maxCode.Substring(4), out int current);
                nextNumber = current + 1;
            }

            return $"{prefix}-{nextNumber:D4}";
        }
    }
}