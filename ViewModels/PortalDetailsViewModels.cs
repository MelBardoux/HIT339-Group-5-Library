namespace LibrarySystem.ViewModels
{
    public class SearchBookDetailsViewModel
    {
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? PublicationYear { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? OtherGenre { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SearchToyDetailsViewModel
    {
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AgeDisplay { get; set; } = string.Empty;
        public bool BatteryRequired { get; set; }
        public List<string> Types { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class SearchMusicDetailsViewModel
    {
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public string? ReleaseYear { get; set; }
        public List<string> Artists { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }
}