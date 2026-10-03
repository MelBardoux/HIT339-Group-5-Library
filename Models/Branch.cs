using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}