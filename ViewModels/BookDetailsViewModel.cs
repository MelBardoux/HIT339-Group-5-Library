namespace LibrarySystem.ViewModels
{
    public class BookDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int? PublicationYear { get; set; }
        public bool PublicationYearUnknown { get; set; }
        public List<string> Genres { get; set; } = new();
        public bool OtherGenre { get; set; }
        public string? OtherGenreText { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly DateAdded { get; set; }
    }
}