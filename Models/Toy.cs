using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class Toy : Item
    {
        [Range(0, 18)]
        public int MinimumAge { get; set; }
        public bool BatteryRequired { get; set; }

        public string AgeDisplay => $"{MinimumAge}+";

        public ICollection<ToyType> Types { get; set; }
    }
}