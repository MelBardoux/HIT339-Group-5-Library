using System.ComponentModel.DataAnnotations;
using LibrarySystem.Models;
using LibrarySystem.Validation;

namespace LibrarySystem.ViewModels
{
    public class BorrowerEditViewModel
    {
        public int Id { get; set; }

        public string LibraryCard { get; set; }

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

        public BorrowerStatus Status { get; set; }
    }
}