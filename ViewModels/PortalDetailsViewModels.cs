namespace LibrarySystem.ViewModels
{
    public class SearchBookDetailsViewModel
    {
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string? PublicationYear { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? OtherGenre { get; set; }
        public string Status { get; set; }
    }

    public class SearchToyDetailsViewModel
    {
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AgeDisplay { get; set; }
        public bool BatteryRequired { get; set; }
        public List<string> Types { get; set; } = new();
        public string Status { get; set; }
    }

    public class SearchMusicDetailsViewModel
    {
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AlbumTitle { get; set; }
        public string? ReleaseYear { get; set; }
        public List<string> Artists { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public string Status { get; set; }
    }
}