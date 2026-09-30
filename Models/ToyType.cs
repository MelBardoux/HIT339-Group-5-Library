namespace LibrarySystem.Models
{
    public class ToyType
    {
        public ICollection<Toy> Toys { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
