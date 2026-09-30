using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;

namespace LibrarySystem.Data

/// Application database context. Extends IdentityDbContext for authentication.
/// Contains DbSets for all entity types including Items (TPH), Borrowers, Loans, and lookup tables.
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Toy> Toys { get; set; }
        public DbSet<Music> Music { get; set; }

        public DbSet<Author> Authors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<ToyType> ToyTypes { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<MusicGenre> MusicGenres { get; set; }
        public DbSet<MusicFormat> MusicFormats { get; set; }
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<Loan> Loans { get; set; }
    }
}