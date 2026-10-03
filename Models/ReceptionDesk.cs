using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class ReceptionDesk
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
    }
}