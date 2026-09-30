namespace LibrarySystem.ViewModels
{
    public class BookDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AuthorName { get; set; }
        public int? PublicationYear { get; set; }
        public bool PublicationYearUnknown { get; set; }
        public List<string> Genres { get; set; } = new();
        public bool OtherGenre { get; set; }
        public string? OtherGenreText { get; set; }
        public string Status { get; set; }
        public DateOnly DateAdded { get; set; }
    }
}