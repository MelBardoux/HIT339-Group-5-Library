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
        public int? SelectedBranchId { get; set; }
        public string SelectedSort { get; set; } = "name";

        public List<BookGenre> BookGenres { get; set; } = new();
        public List<ToyType> ToyTypes { get; set; } = new();
        public List<MusicFormat> MusicFormats { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();

        public List<PortalBookViewModel> Books { get; set; } = new();
        public List<PortalToyViewModel> Toys { get; set; } = new();
        public List<PortalMusicViewModel> Music { get; set; } = new();
    }

    public class SearchResultViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
    }

    public class PortalBookViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? PublicationYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
    }

    public class PortalToyViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AgeDisplay { get; set; } = string.Empty;
        public bool BatteryRequired { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public List<string> Types { get; set; } = new();
    }

    public class PortalMusicViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public string Artists { get; set; } = string.Empty;
        public string? ReleaseYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public List<string> Formats { get; set; } = new();
        public List<string> Genres { get; set; } = new();
    }
}