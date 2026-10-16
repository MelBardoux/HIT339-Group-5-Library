namespace LibrarySystem.ViewModels
{
    public class BookDeleteViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}