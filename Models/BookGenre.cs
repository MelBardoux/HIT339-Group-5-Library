namespace LibrarySystem.Models
{
    public class BookGenre
    {
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}