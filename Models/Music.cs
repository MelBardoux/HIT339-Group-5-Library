using System.ComponentModel.DataAnnotations;
using LibrarySystem.Validation;

namespace LibrarySystem.Models
{
    public class Music : Item
    {
        [Required]
        [StringLength(100)]
        public string AlbumTitle { get; set; }

        [YearRange(1450, nameof(ReleaseYearUnknown))]
        public int? ReleaseYear { get; set; }

        public bool ReleaseYearUnknown { get; set; }

        public ICollection<Artist> Artists { get; set; }
        public ICollection<MusicGenre> Genres { get; set; }
        public ICollection<MusicFormat> Formats { get; set; }
    }
}