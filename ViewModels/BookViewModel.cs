using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Validation;

namespace LibrarySystem.ViewModels

/// ViewModel for the Book Create form. Contains validation attributes and
/// MVVM methods for loading genres, resolving authors, and mapping to a Book entity.
/// Business logic is kept in the ViewModel, controller only coordinates flow.
{
    public class BookViewModel
    {
        public int Id { get; set; }

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

        public List<GenreCheckboxViewModel> AvailableGenres { get; set; } = new();

        public bool OtherGenreSelected { get; set; }
        public string? OtherGenreName { get; set; }

        /// Loads all available book genres from the database for checkbox display.
        public async Task LoadGenres(ApplicationDbContext context)
        {
            AvailableGenres = await context.BookGenres
                .Select(g => new GenreCheckboxViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsSelected = false
                })
                .ToListAsync();
        }

        /// Reloads genres preserving the user's selections after a failed validation.
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

        /// Finds an existing author by name or creates a new one if not found.
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

        /// Maps the ViewModel data to a new Book entity, including auto-generating
        /// the library code and resolving genre and author relationships.
        public async Task<Book> ToBook(ApplicationDbContext context)
        {
            var author = await ResolveAuthor(context);

            var libraryCode = await Item.GenerateLibraryCode<Book>(context, "BKS");

            var selectedGenreIds = AvailableGenres
                .Where(g => g.IsSelected)
                .Select(g => g.Id)
                .ToList();

            var book = new Book
            {
                Name = Name,
                Description = Description,
                PublicationYear = PublicationYear,
                PublicationYearUnknown = PublicationYearUnknown,
                AuthorId = author.Id,
                Status = ItemStatus.Available,
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                LibraryCode = libraryCode,
                Genres = await context.BookGenres
                    .Where(g => selectedGenreIds.Contains(g.Id))
                    .ToListAsync()
            };

            if (OtherGenreSelected && !string.IsNullOrWhiteSpace(OtherGenreName))
            {
                book.OtherGenre = true;
                book.OtherGenreText = OtherGenreName;
            }

            return book;
        }
    }

    public class GenreCheckboxViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}