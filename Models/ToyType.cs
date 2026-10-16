namespace LibrarySystem.Models
{
    public class ToyType
    {
        public ICollection<Toy> Toys { get; set; } = new List<Toy>();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
