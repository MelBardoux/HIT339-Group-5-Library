namespace LibrarySystem.Models
{
    public class Artist
    {
        public ICollection<Music> Music { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
