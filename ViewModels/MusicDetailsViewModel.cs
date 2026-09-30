namespace LibrarySystem.ViewModels
{
    public class MusicDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AlbumTitle { get; set; }
        public int? ReleaseYear { get; set; }
        public bool ReleaseYearUnknown { get; set; }
        public List<string> Artists { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public string Status { get; set; }
        public DateOnly DateAdded { get; set; }
    }
}