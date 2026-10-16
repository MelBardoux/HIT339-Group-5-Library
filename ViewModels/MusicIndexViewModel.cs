namespace LibrarySystem.ViewModels
{
    public class MusicIndexViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public string Artists { get; set; } = string.Empty;
        public int? ReleaseYear { get; set; }
        public bool ReleaseYearUnknown { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool OnOrder { get; set; }
        public string? ReservedForCard { get; set; }
    }
}