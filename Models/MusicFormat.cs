namespace LibrarySystem.Models
{
    public class MusicFormat
    {
        public ICollection<Music> Music { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
