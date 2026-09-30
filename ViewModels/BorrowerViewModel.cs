using System.ComponentModel.DataAnnotations;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Validation;

namespace LibrarySystem.ViewModels
{
    public class BorrowerViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [MinimumAge(12)]
        public DateOnly? DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [StringLength(250)]
        public string Address { get; set; }

        public async Task<Borrower> ToBorrower(ApplicationDbContext context)
        {
            var libraryCard = await Borrower.GenerateLibraryCard(context);

            return new Borrower
            {
                Name = Name,
                DateOfBirth = DateOfBirth.Value,
                Email = Email,
                Phone = Phone,
                Address = Address,
                LibraryCard = libraryCard,
                Status = BorrowerStatus.Active
            };
        }
    }
}