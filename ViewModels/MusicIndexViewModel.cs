namespace LibrarySystem.ViewModels
{
    public class MusicIndexViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string AlbumTitle { get; set; }
        public string Artists { get; set; }
        public int? ReleaseYear { get; set; }
        public bool ReleaseYearUnknown { get; set; }
        public string Status { get; set; }
        public bool OnOrder { get; set; }
        public string? ReservedForCard { get; set; }
    }
}