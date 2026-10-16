namespace LibrarySystem.ViewModels
{
    public class BookIndexViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int? PublicationYear { get; set; }
        public bool PublicationYearUnknown { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool OnOrder { get; set; }
        public string? ReservedForCard { get; set; }
    }
}