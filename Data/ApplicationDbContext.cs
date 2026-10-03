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

        public DbSet<Branch> Branches {  get; set; } //Branch update for multi-location expansion
        public DbSet<ReceptionDesk> ReceptionDesks { get; set; } //ReceptionDesk update for reception desks in multi branch location feature
        public DbSet<ItemTransfer> ItemTransfers { get; set; } //ItemTransfers update for multi branch expansion

        public DbSet<Author> Authors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<ToyType> ToyTypes { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<MusicGenre> MusicGenres { get; set; }
        public DbSet<MusicFormat> MusicFormats { get; set; }
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<Loan> Loans { get; set; }


        // OnModelCreating method to configure the multi-branch relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Item -> Branch
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Branch)
                .WithMany(b => b.Items)
                .HasForeignKey(i => i.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReceptionDesk -> Branch
            modelBuilder.Entity<ReceptionDesk>()
                .HasOne(d => d.Branch)
                .WithMany(b => b.ReceptionDesks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemTransfer -> Item
            modelBuilder.Entity<ItemTransfer>()
                .HasOne(t => t.Item)
                .WithMany()
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemTransfer -> FromBranch
            modelBuilder.Entity<ItemTransfer>()
                .HasOne(t => t.FromBranch)
                .WithMany()
                .HasForeignKey(t => t.FromBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemTransfer -> ToBranch
            modelBuilder.Entity<ItemTransfer>()
                .HasOne(t => t.ToBranch)
                .WithMany()
                .HasForeignKey(t => t.ToBranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}