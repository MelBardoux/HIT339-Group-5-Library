namespace LibrarySystem.DTOs
{
    public class CatalogueCategoriesDto
    {
        public List<string> BookGenres { get; set; } = new();

        public List<string> ToyTypes { get; set; } = new();

        public List<string> MusicGenres { get; set; } = new();
    }
}