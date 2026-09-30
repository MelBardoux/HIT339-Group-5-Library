using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Validation;

namespace LibrarySystem.ViewModels
{
    public class BookEditViewModel
    {
        public int Id { get; set; }

        public string LibraryCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [YearRange(1450, nameof(PublicationYearUnknown))]
        public int? PublicationYear { get; set; }

        public bool PublicationYearUnknown { get; set; }

        [Required]
        public string AuthorName { get; set; }

        public int? AuthorId { get; set; }

        public ItemStatus Status { get; set; }

        public bool OnOrder { get; set; }

        public List<GenreCheckboxViewModel> AvailableGenres { get; set; } = new();

        public bool OtherGenreSelected { get; set; }
        public string? OtherGenreName { get; set; }

        public async Task LoadGenres(ApplicationDbContext context, ICollection<BookGenre> selectedGenres)
        {
            var selectedIds = selectedGenres.Select(g => g.Id).ToList();

            AvailableGenres = await context.BookGenres
                .Select(g => new GenreCheckboxViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsSelected = selectedIds.Contains(g.Id)
                })
                .ToListAsync();
        }

        public async Task ReloadGenres(ApplicationDbContext context)
        {
            var selectedIds = AvailableGenres
                .Where(g => g.IsSelected)
                .Select(g => g.Id)
                .ToList();

            AvailableGenres = await context.BookGenres
                .Select(g => new GenreCheckboxViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsSelected = selectedIds.Contains(g.Id)
                })
                .ToListAsync();
        }

        public async Task<Author> ResolveAuthor(ApplicationDbContext context)
        {
            var author = await context.Authors
                .FirstOrDefaultAsync(a => a.Name == AuthorName);

            if (author == null)
            {
                author = new Author { Name = AuthorName };
                context.Authors.Add(author);
                await context.SaveChangesAsync();
            }

            return author;
        }

        public async Task UpdateBook(ApplicationDbContext context, Book book)
        {
            var author = await ResolveAuthor(context);

            book.Name = Name;
            book.Description = Description;
            book.PublicationYear = PublicationYear;
            book.PublicationYearUnknown = PublicationYearUnknown;
            book.AuthorId = author.Id;
            book.Status = Status;
            book.OnOrder = OnOrder;

            var selectedGenreIds = AvailableGenres
                .Where(g => g.IsSelected)
                .Select(g => g.Id)
                .ToList();

            book.Genres = await context.BookGenres
                .Where(g => selectedGenreIds.Contains(g.Id))
                .ToListAsync();

            if (OtherGenreSelected && !string.IsNullOrWhiteSpace(OtherGenreName))
            {
                book.OtherGenre = true;
                book.OtherGenreText = OtherGenreName;
            }
            else
            {
                book.OtherGenre = false;
                book.OtherGenreText = null;
            }
        }
    }
}