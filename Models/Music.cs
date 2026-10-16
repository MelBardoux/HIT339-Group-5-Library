using System.ComponentModel.DataAnnotations;
using LibrarySystem.Validation;

namespace LibrarySystem.Models
{
    public class Music : Item
    {
        [Required]
        [StringLength(100)]
        public string AlbumTitle { get; set; } = string.Empty;

        [YearRange(1450, nameof(ReleaseYearUnknown))]
        public int? ReleaseYear { get; set; }

        public bool ReleaseYearUnknown { get; set; }

        public ICollection<Artist> Artists { get; set; } = new List<Artist>();
        public ICollection<MusicGenre> Genres { get; set; } = new List<MusicGenre>();
        public ICollection<MusicFormat> Formats { get; set; } = new List<MusicFormat>();
    }
}