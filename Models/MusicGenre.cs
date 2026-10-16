namespace LibrarySystem.Models
{
    public class MusicGenre
    {
        public ICollection<Music> Music { get; set; } = new List<Music>();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
