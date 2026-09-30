namespace LibrarySystem.ViewModels
{
    public class BookIndexViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string AuthorName { get; set; }
        public int? PublicationYear { get; set; }
        public bool PublicationYearUnknown { get; set; }
        public string Status { get; set; }
        public bool OnOrder { get; set; }
        public string? ReservedForCard { get; set; }
    }
}