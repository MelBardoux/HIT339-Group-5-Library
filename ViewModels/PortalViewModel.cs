using LibrarySystem.Models;

namespace LibrarySystem.ViewModels
{
    public class PortalViewModel
    {
        public string? SearchTerm { get; set; }
        public bool IsSearching { get; set; }
        public List<SearchResultViewModel> SearchResults { get; set; } = new();

        public string ActiveTab { get; set; } = "books";
        public int? SelectedGenreId { get; set; }
        public int? SelectedTypeId { get; set; }
        public int? SelectedFormatId { get; set; }
        public string SelectedSort { get; set; } = "name";

        public List<BookGenre> BookGenres { get; set; } = new();
        public List<ToyType> ToyTypes { get; set; } = new();
        public List<MusicFormat> MusicFormats { get; set; } = new();

        public List<PortalBookViewModel> Books { get; set; } = new();
        public List<PortalToyViewModel> Toys { get; set; } = new();
        public List<PortalMusicViewModel> Music { get; set; } = new();
    }

    public class SearchResultViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
    }

    public class PortalBookViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string AuthorName { get; set; }
        public string? PublicationYear { get; set; }
        public string Status { get; set; }
        public List<string> Genres { get; set; } = new();
    }

    public class PortalToyViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string AgeDisplay { get; set; }
        public bool BatteryRequired { get; set; }
        public string Status { get; set; }
        public List<string> Types { get; set; } = new();
    }

    public class PortalMusicViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string AlbumTitle { get; set; }
        public string Artists { get; set; }
        public string? ReleaseYear { get; set; }
        public string Status { get; set; }
        public List<string> Formats { get; set; } = new();
        public List<string> Genres { get; set; } = new();
    }

}