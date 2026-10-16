namespace LibrarySystem.ViewModels
{
    public class MusicDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public int? ReleaseYear { get; set; }
        public bool ReleaseYearUnknown { get; set; }
        public List<string> Artists { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateOnly DateAdded { get; set; }
    }
}