using System.ComponentModel.DataAnnotations;
using LibrarySystem.Validation;

namespace LibrarySystem.Models
{
    public class Book : Item
    {
        [YearRange(1450, nameof(PublicationYearUnknown))]
        public int? PublicationYear { get; set; }
        public bool PublicationYearUnknown { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public ICollection<BookGenre> Genres { get; set; }
        public bool OtherGenre { get; set; }
        public string? OtherGenreText { get; set; }
    }
}