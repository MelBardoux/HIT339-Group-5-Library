namespace LibrarySystem.Models
{
    public class BookGenre
    {
        public ICollection<Book> Books { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}