namespace LibrarySystem.Models
{
    public class MusicGenre
    {
        public ICollection<Music> Music { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
