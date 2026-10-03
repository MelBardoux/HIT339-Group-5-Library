using Microsoft.AspNetCore.Identity;
using LibrarySystem.Models;

namespace LibrarySystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed roles
            string[] roles = { "Admin", "Reception", "Manager" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin user
            if (await userManager.FindByEmailAsync("admin@library.com") == null)
            {
                var admin = new IdentityUser
                {
                    UserName = "admin@library.com",
                    Email = "admin@library.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed Reception user
            if (await userManager.FindByEmailAsync("reception@library.com") == null)
            {
                var reception = new IdentityUser
                {
                    UserName = "reception@library.com",
                    Email = "reception@library.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(reception, "Reception123!");
                await userManager.AddToRoleAsync(reception, "Reception");
            }

            // Seed Manager user
            if (await userManager.FindByEmailAsync("manager@library.com") == null)
            {
                var manager = new IdentityUser
                {
                    UserName = "manager@library.com",
                    Email = "manager@library.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(manager, "Manager123!");
                await userManager.AddToRoleAsync(manager, "Manager");
            }

            // Seed Book Genres
            if (!context.BookGenres.Any())
            {
                context.BookGenres.AddRange(
                    new BookGenre { Name = "Fiction" },
                    new BookGenre { Name = "Non-Fiction" },
                    new BookGenre { Name = "Science Fiction" },
                    new BookGenre { Name = "Fantasy" },
                    new BookGenre { Name = "Mystery" },
                    new BookGenre { Name = "Romance" },
                    new BookGenre { Name = "Horror" },
                    new BookGenre { Name = "Biography" },
                    new BookGenre { Name = "History" },
                    new BookGenre { Name = "Children's" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Toy Types
            if (!context.ToyTypes.Any())
            {
                context.ToyTypes.AddRange(
                    new ToyType { Name = "Board Game" },
                    new ToyType { Name = "Puzzle" },
                    new ToyType { Name = "Building Set" },
                    new ToyType { Name = "Action Figure" },
                    new ToyType { Name = "Stuffed Animal" },
                    new ToyType { Name = "Educational" },
                    new ToyType { Name = "Outdoor" },
                    new ToyType { Name = "Electronic" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Music Genres
            if (!context.MusicGenres.Any())
            {
                context.MusicGenres.AddRange(
                    new MusicGenre { Name = "Rock" },
                    new MusicGenre { Name = "Pop" },
                    new MusicGenre { Name = "Jazz" },
                    new MusicGenre { Name = "Classical" },
                    new MusicGenre { Name = "Hip Hop" },
                    new MusicGenre { Name = "Country" },
                    new MusicGenre { Name = "Electronic" },
                    new MusicGenre { Name = "R&B" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Music Formats
            if (!context.MusicFormats.Any())
            {
                context.MusicFormats.AddRange(
                    new MusicFormat { Name = "CD" },
                    new MusicFormat { Name = "Vinyl" },
                    new MusicFormat { Name = "Cassette" },
                    new MusicFormat { Name = "DVD" },
                    new MusicFormat { Name = "Blu-Ray" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Authors
            if (!context.Authors.Any())
            {
                context.Authors.AddRange(
                    new Author { Name = "Harper Lee" },
                    new Author { Name = "Stephen Hawking" },
                    new Author { Name = "J.R.R. Tolkien" },
                    new Author { Name = "Agatha Christie" },
                    new Author { Name = "Jeff Kinney" },
                    new Author { Name = "Frank Herbert" },
                    new Author { Name = "Jane Austen" },
                    new Author { Name = "Bram Stoker" },
                    new Author { Name = "Nelson Mandela" },
                    new Author { Name = "Yuval Noah Harari" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Artists
            if (!context.Artists.Any())
            {
                context.Artists.AddRange(
                    new Artist { Name = "The Beatles" },
                    new Artist { Name = "Michael Jackson" },
                    new Artist { Name = "Miles Davis" },
                    new Artist { Name = "AC/DC" },
                    new Artist { Name = "Lauryn Hill" },
                    new Artist { Name = "Dolly Parton" },
                    new Artist { Name = "Daft Punk" },
                    new Artist { Name = "Beyoncé" },
                    new Artist { Name = "Tchaikovsky" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Borrowers
            if (!context.Borrowers.Any())
            {
                context.Borrowers.AddRange(
                    new Borrower
                    {
                        LibraryCard = "BRW-0001",
                        Name = "Jane Smith",
                        DateOfBirth = new DateOnly(1990, 3, 15),
                        Email = "jane.smith@email.com",
                        Phone = "0412345678",
                        Address = "12 Main Street, Darwin",
                        Status = BorrowerStatus.Active
                    },
                    new Borrower
                    {
                        LibraryCard = "BRW-0002",
                        Name = "Tom Wilson",
                        DateOfBirth = new DateOnly(1985, 7, 22),
                        Email = "tom.wilson@email.com",
                        Phone = "0423456789",
                        Address = "45 River Road, Darwin",
                        Status = BorrowerStatus.Active
                    },
                    new Borrower
                    {
                        LibraryCard = "BRW-0003",
                        Name = "Sarah Chen",
                        DateOfBirth = new DateOnly(2010, 11, 1),
                        Email = "sarah.chen@email.com",
                        Phone = "0434567890",
                        Address = "78 Park Avenue, Palmerston",
                        Status = BorrowerStatus.Active
                    },
                    new Borrower
                    {
                        LibraryCard = "BRW-0004",
                        Name = "Mike Johnson",
                        DateOfBirth = new DateOnly(1978, 12, 3),
                        Email = "mike.johnson@email.com",
                        Phone = "0445678901",
                        Address = "23 Beach Drive, Darwin",
                        Status = BorrowerStatus.Suspended
                    },
                    new Borrower
                    {
                        LibraryCard = "BRW-0005",
                        Name = "Emily Davis",
                        DateOfBirth = new DateOnly(1995, 6, 18),
                        Email = "emily.davis@email.com",
                        Phone = "0456789012",
                        Address = "56 Hill Street, Katherine",
                        Status = BorrowerStatus.Active
                    },
                    new Borrower
                    {
                        LibraryCard = "BRW-0006",
                        Name = "Alex Turner",
                        DateOfBirth = new DateOnly(2000, 1, 30),
                        Email = "alex.turner@email.com",
                        Phone = "0467890123",
                        Address = "89 Lake Road, Alice Springs",
                        Status = BorrowerStatus.Suspended
                    }
                );
                await context.SaveChangesAsync();
            }


            // Seed Reception Desks
            if (!context.ReceptionDesks.Any())
            {
                var darwin = context.Branches.FirstOrDefault(b => b.Name == "Darwin");
                var sydney = context.Branches.FirstOrDefault(b => b.Name == "Sydney");
                var brisbane = context.Branches.FirstOrDefault(b => b.Name == "Brisbane");

                if (darwin == null || sydney == null || brisbane == null)
                {
                    throw new InvalidOperationException(
                        "Required library branches were not found. Run the AddMultiBranch migration first.");
                }

                context.ReceptionDesks.AddRange(
                    new ReceptionDesk
                    {
                        Name = "Reception Desk 1",
                        BranchId = darwin.Id
                    },
                    new ReceptionDesk
                    {
                        Name = "Reception Desk 2",
                        BranchId = darwin.Id
                    },
                    new ReceptionDesk
                    {
                        Name = "Reception Desk 1",
                        BranchId = sydney.Id
                    },
                    new ReceptionDesk
                    {
                        Name = "Reception Desk 2",
                        BranchId = sydney.Id
                    },
                    new ReceptionDesk
                    {
                        Name = "Reception Desk 1",
                        BranchId = brisbane.Id
                    },
                    new ReceptionDesk
                    {
                        Name = "Reception Desk 2",
                        BranchId = brisbane.Id
                    }
                );

                await context.SaveChangesAsync();
            }


            // Seed Books
            if (!context.Books.Any())
            {
                var authors = context.Authors.ToList();
                var genres = context.BookGenres.ToList();
                var borrowers = context.Borrowers.ToList();

                var fiction = genres.First(g => g.Name == "Fiction");
                var nonFiction = genres.First(g => g.Name == "Non-Fiction");
                var sciFi = genres.First(g => g.Name == "Science Fiction");
                var fantasy = genres.First(g => g.Name == "Fantasy");
                var mystery = genres.First(g => g.Name == "Mystery");
                var romance = genres.First(g => g.Name == "Romance");
                var horror = genres.First(g => g.Name == "Horror");
                var biography = genres.First(g => g.Name == "Biography");
                var history = genres.First(g => g.Name == "History");
                var childrens = genres.First(g => g.Name == "Children's");

                var sarahChen = borrowers.First(b => b.Name == "Sarah Chen");

                context.Books.AddRange(
                    new Book
                    {
                        Name = "To Kill a Mockingbird",
                        Description = "A novel about racial injustice in the American South.",
                        LibraryCode = "BKS-0001",
                        Status = ItemStatus.Borrowed,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-60)),
                        PublicationYear = 1960,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Harper Lee").Id,
                        Genres = new List<BookGenre> { fiction }
                    },
                    new Book
                    {
                        Name = "A Brief History of Time",
                        Description = "A landmark volume in science writing exploring the nature of time and the universe.",
                        LibraryCode = "BKS-0002",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-55)),
                        PublicationYear = 1988,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Stephen Hawking").Id,
                        Genres = new List<BookGenre> { nonFiction }
                    },
                    new Book
                    {
                        Name = "The Hobbit",
                        Description = "A fantasy novel about the adventures of Bilbo Baggins.",
                        LibraryCode = "BKS-0003",
                        Status = ItemStatus.Damaged,
                        OnOrder = true,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-50)),
                        PublicationYear = 1937,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "J.R.R. Tolkien").Id,
                        Genres = new List<BookGenre> { fantasy, fiction }
                    },
                    new Book
                    {
                        Name = "Murder on the Orient Express",
                        Description = "A detective novel featuring Hercule Poirot solving a murder on a train.",
                        LibraryCode = "BKS-0004",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)),
                        PublicationYear = 1934,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Agatha Christie").Id,
                        Genres = new List<BookGenre> { mystery }
                    },
                    new Book
                    {
                        Name = "Diary of a Wimpy Kid",
                        Description = "A children's novel about the struggles of middle school life.",
                        LibraryCode = "BKS-0005",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)),
                        PublicationYear = 2007,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Jeff Kinney").Id,
                        Genres = new List<BookGenre> { childrens, fiction }
                    },
                    new Book
                    {
                        Name = "Dune",
                        Description = "A science fiction epic set on the desert planet Arrakis.",
                        LibraryCode = "BKS-0006",
                        Status = ItemStatus.Borrowed,
                        ReservedForBorrowerId = sarahChen.Id,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-35)),
                        PublicationYear = 1965,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Frank Herbert").Id,
                        Genres = new List<BookGenre> { sciFi }
                    },
                    new Book
                    {
                        Name = "Pride and Prejudice",
                        Description = "A romantic novel about manners, marriage, and society in Regency-era England.",
                        LibraryCode = "BKS-0007",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                        PublicationYear = 1813,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Jane Austen").Id,
                        Genres = new List<BookGenre> { romance, fiction }
                    },
                    new Book
                    {
                        Name = "Dracula",
                        Description = "A gothic horror novel about the vampire Count Dracula.",
                        LibraryCode = "BKS-0008",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                        PublicationYear = 1897,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Bram Stoker").Id,
                        Genres = new List<BookGenre> { horror, fiction }
                    },
                    new Book
                    {
                        Name = "Long Walk to Freedom",
                        Description = "The autobiography of Nelson Mandela, chronicling his life and struggle against apartheid.",
                        LibraryCode = "BKS-0009",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                        PublicationYear = 1994,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Nelson Mandela").Id,
                        Genres = new List<BookGenre> { biography, nonFiction }
                    },
                    new Book
                    {
                        Name = "Sapiens",
                        Description = "A brief history of humankind from the Stone Age to the present.",
                        LibraryCode = "BKS-0010",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                        PublicationYear = 2011,
                        PublicationYearUnknown = false,
                        AuthorId = authors.First(a => a.Name == "Yuval Noah Harari").Id,
                        Genres = new List<BookGenre> { history, nonFiction }
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Toys
            if (!context.Toys.Any())
            {
                var toyTypes = context.ToyTypes.ToList();
                var borrowers = context.Borrowers.ToList();

                var boardGame = toyTypes.First(t => t.Name == "Board Game");
                var puzzle = toyTypes.First(t => t.Name == "Puzzle");
                var buildingSet = toyTypes.First(t => t.Name == "Building Set");
                var actionFigure = toyTypes.First(t => t.Name == "Action Figure");
                var stuffedAnimal = toyTypes.First(t => t.Name == "Stuffed Animal");
                var educational = toyTypes.First(t => t.Name == "Educational");
                var outdoor = toyTypes.First(t => t.Name == "Outdoor");
                var electronic = toyTypes.First(t => t.Name == "Electronic");

                var sarahChen = borrowers.First(b => b.Name == "Sarah Chen");

                context.Toys.AddRange(
                    new Toy
                    {
                        Name = "LEGO Classic Set",
                        Description = "A creative building set with assorted bricks and pieces.",
                        LibraryCode = "TOY-0001",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-50)),
                        MinimumAge = 4,
                        BatteryRequired = false,
                        Types = new List<ToyType> { buildingSet }
                    },
                    new Toy
                    {
                        Name = "Monopoly",
                        Description = "The classic property trading board game.",
                        LibraryCode = "TOY-0002",
                        Status = ItemStatus.Damaged,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)),
                        MinimumAge = 8,
                        BatteryRequired = false,
                        Types = new List<ToyType> { boardGame }
                    },
                    new Toy
                    {
                        Name = "Remote Control Car",
                        Description = "A fast remote control car for indoor and outdoor use.",
                        LibraryCode = "TOY-0003",
                        Status = ItemStatus.Lost,
                        OnOrder = true,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)),
                        MinimumAge = 6,
                        BatteryRequired = true,
                        Types = new List<ToyType> { electronic, outdoor }
                    },
                    new Toy
                    {
                        Name = "Floor Puzzle World Map",
                        Description = "A large floor puzzle featuring a colourful world map.",
                        LibraryCode = "TOY-0004",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-35)),
                        MinimumAge = 3,
                        BatteryRequired = false,
                        Types = new List<ToyType> { puzzle, educational }
                    },
                    new Toy
                    {
                        Name = "Spider-Man Action Figure",
                        Description = "A poseable Spider-Man action figure with web accessories.",
                        LibraryCode = "TOY-0005",
                        Status = ItemStatus.Borrowed,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                        MinimumAge = 4,
                        BatteryRequired = false,
                        Types = new List<ToyType> { actionFigure }
                    },
                    new Toy
                    {
                        Name = "Teddy Bear",
                        Description = "A soft and cuddly teddy bear.",
                        LibraryCode = "TOY-0006",
                        Status = ItemStatus.Destroyed,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                        MinimumAge = 0,
                        BatteryRequired = false,
                        Types = new List<ToyType> { stuffedAnimal }
                    },
                    new Toy
                    {
                        Name = "Frisbee",
                        Description = "A classic flying disc for outdoor play.",
                        LibraryCode = "TOY-0007",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                        MinimumAge = 5,
                        BatteryRequired = false,
                        Types = new List<ToyType> { outdoor }
                    },
                    new Toy
                    {
                        Name = "LeapFrog Tablet",
                        Description = "An interactive learning tablet for young children.",
                        LibraryCode = "TOY-0008",
                        Status = ItemStatus.Borrowed,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                        MinimumAge = 3,
                        BatteryRequired = true,
                        Types = new List<ToyType> { electronic, educational }
                    },
                    new Toy
                    {
                        Name = "Scrabble",
                        Description = "The classic word-building board game.",
                        LibraryCode = "TOY-0009",
                        Status = ItemStatus.Borrowed,
                        ReservedForBorrowerId = sarahChen.Id,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                        MinimumAge = 10,
                        BatteryRequired = false,
                        Types = new List<ToyType> { boardGame }
                    },
                    new Toy
                    {
                        Name = "Rubik's Cube",
                        Description = "A 3D combination puzzle for all ages.",
                        LibraryCode = "TOY-0010",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-8)),
                        MinimumAge = 8,
                        BatteryRequired = false,
                        Types = new List<ToyType> { puzzle }
                    },
                    new Toy
                    {
                        Name = "Plush Dinosaur",
                        Description = "A soft and cuddly dinosaur plush toy.",
                        LibraryCode = "TOY-0011",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                        MinimumAge = 0,
                        BatteryRequired = false,
                        Types = new List<ToyType> { stuffedAnimal }
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Music
            if (!context.Music.Any())
            {
                var artists = context.Artists.ToList();
                var musicGenres = context.MusicGenres.ToList();
                var formats = context.MusicFormats.ToList();

                var rock = musicGenres.First(g => g.Name == "Rock");
                var pop = musicGenres.First(g => g.Name == "Pop");
                var jazz = musicGenres.First(g => g.Name == "Jazz");
                var classical = musicGenres.First(g => g.Name == "Classical");
                var hipHop = musicGenres.First(g => g.Name == "Hip Hop");
                var country = musicGenres.First(g => g.Name == "Country");
                var electronicGenre = musicGenres.First(g => g.Name == "Electronic");
                var rnb = musicGenres.First(g => g.Name == "R&B");

                var cd = formats.First(f => f.Name == "CD");
                var vinyl = formats.First(f => f.Name == "Vinyl");
                var cassette = formats.First(f => f.Name == "Cassette");
                var dvd = formats.First(f => f.Name == "DVD");
                var bluray = formats.First(f => f.Name == "Blu-Ray");

                context.Music.AddRange(
                    new Music
                    {
                        Name = "Abbey Road",
                        Description = "The eleventh studio album by the English rock band The Beatles.",
                        LibraryCode = "MUS-0001",
                        Status = ItemStatus.Borrowed,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-50)),
                        AlbumTitle = "Abbey Road",
                        ReleaseYear = 1969,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "The Beatles") },
                        Genres = new List<MusicGenre> { rock },
                        Formats = new List<MusicFormat> { vinyl, cd }
                    },
                    new Music
                    {
                        Name = "Thriller",
                        Description = "The sixth studio album by American singer Michael Jackson.",
                        LibraryCode = "MUS-0002",
                        Status = ItemStatus.Borrowed,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)),
                        AlbumTitle = "Thriller",
                        ReleaseYear = 1982,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Michael Jackson") },
                        Genres = new List<MusicGenre> { pop },
                        Formats = new List<MusicFormat> { cd }
                    },
                    new Music
                    {
                        Name = "Kind of Blue",
                        Description = "A studio album by American jazz trumpeter Miles Davis.",
                        LibraryCode = "MUS-0003",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)),
                        AlbumTitle = "Kind of Blue",
                        ReleaseYear = 1959,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Miles Davis") },
                        Genres = new List<MusicGenre> { jazz },
                        Formats = new List<MusicFormat> { vinyl }
                    },
                    new Music
                    {
                        Name = "Back in Black",
                        Description = "The seventh studio album by Australian rock band AC/DC.",
                        LibraryCode = "MUS-0004",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-35)),
                        AlbumTitle = "Back in Black",
                        ReleaseYear = 1980,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "AC/DC") },
                        Genres = new List<MusicGenre> { rock },
                        Formats = new List<MusicFormat> { cd, vinyl }
                    },
                    new Music
                    {
                        Name = "The Miseducation of Lauryn Hill",
                        Description = "The debut solo studio album by American singer Lauryn Hill.",
                        LibraryCode = "MUS-0005",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                        AlbumTitle = "The Miseducation of Lauryn Hill",
                        ReleaseYear = 1998,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Lauryn Hill") },
                        Genres = new List<MusicGenre> { hipHop },
                        Formats = new List<MusicFormat> { cd }
                    },
                    new Music
                    {
                        Name = "Jolene",
                        Description = "The thirteenth solo studio album by American singer-songwriter Dolly Parton.",
                        LibraryCode = "MUS-0006",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                        AlbumTitle = "Jolene",
                        ReleaseYear = 1974,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Dolly Parton") },
                        Genres = new List<MusicGenre> { country },
                        Formats = new List<MusicFormat> { vinyl, cassette }
                    },
                    new Music
                    {
                        Name = "Random Access Memories",
                        Description = "The fourth studio album by French electronic music duo Daft Punk.",
                        LibraryCode = "MUS-0007",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                        AlbumTitle = "Random Access Memories",
                        ReleaseYear = 2013,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Daft Punk") },
                        Genres = new List<MusicGenre> { electronicGenre },
                        Formats = new List<MusicFormat> { cd, vinyl }
                    },
                    new Music
                    {
                        Name = "Lemonade",
                        Description = "The sixth studio album by American singer Beyoncé.",
                        LibraryCode = "MUS-0008",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                        AlbumTitle = "Lemonade",
                        ReleaseYear = 2016,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Beyoncé") },
                        Genres = new List<MusicGenre> { rnb, pop },
                        Formats = new List<MusicFormat> { cd, dvd }
                    },
                    new Music
                    {
                        Name = "The Nutcracker",
                        Description = "A ballet composed by Pyotr Ilyich Tchaikovsky.",
                        LibraryCode = "MUS-0009",
                        Status = ItemStatus.Available,
                        DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                        AlbumTitle = "The Nutcracker",
                        ReleaseYear = 1892,
                        ReleaseYearUnknown = false,
                        Artists = new List<Artist> { artists.First(a => a.Name == "Tchaikovsky") },
                        Genres = new List<MusicGenre> { classical },
                        Formats = new List<MusicFormat> { cd, bluray }
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Loans
            if (!context.Loans.Any())
            {
                var borrowers = context.Borrowers.ToList();
                var items = context.Items.ToList();

                var jane = borrowers.First(b => b.Name == "Jane Smith");
                var tom = borrowers.First(b => b.Name == "Tom Wilson");
                var sarah = borrowers.First(b => b.Name == "Sarah Chen");
                var mike = borrowers.First(b => b.Name == "Mike Johnson");
                var emily = borrowers.First(b => b.Name == "Emily Davis");
                var alex = borrowers.First(b => b.Name == "Alex Turner");

                var mockingbird = items.First(i => i.LibraryCode == "BKS-0001");
                var dune = items.First(i => i.LibraryCode == "BKS-0006");
                var abbeyRoad = items.First(i => i.LibraryCode == "MUS-0001");
                var spiderman = items.First(i => i.LibraryCode == "TOY-0005");
                var leapfrog = items.First(i => i.LibraryCode == "TOY-0008");
                var scrabble = items.First(i => i.LibraryCode == "TOY-0009");

                var today = DateOnly.FromDateTime(DateTime.Now);

                context.Loans.AddRange(
                    // Mike's overdue loan - returned late with fine
                    new Loan
                    {
                        ItemId = mockingbird.Id,
                        BorrowerId = mike.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-45)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-31)),
                        ReturnedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                        Fine = 11.00m
                    },
                    // Alex overdue with Thriller - not returned
                    new Loan
                    {
                        ItemId = items.First(i => i.LibraryCode == "MUS-0002").Id,
                        BorrowerId = alex.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-24)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                        Fine = 8.00m
                    },
                    // Jane currently borrowing To Kill a Mockingbird
                    new Loan
                    {
                        ItemId = mockingbird.Id,
                        BorrowerId = jane.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(9))
                    },
                    // Emily currently borrowing Dune (reserved for Sarah)
                    new Loan
                    {
                        ItemId = dune.Id,
                        BorrowerId = emily.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7))
                    },
                    // Tom currently borrowing Abbey Road
                    new Loan
                    {
                        ItemId = abbeyRoad.Id,
                        BorrowerId = tom.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(11))
                    },
                    // Sarah currently borrowing Spider-Man Action Figure
                    new Loan
                    {
                        ItemId = spiderman.Id,
                        BorrowerId = sarah.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-4)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10))
                    },
                    // Tom currently borrowing LeapFrog Tablet
                    new Loan
                    {
                        ItemId = leapfrog.Id,
                        BorrowerId = tom.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(12))
                    },
                    // Emily currently borrowing Scrabble (reserved for Sarah)
                    new Loan
                    {
                        ItemId = scrabble.Id,
                        BorrowerId = emily.Id,
                        BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-6)),
                        DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(8))
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}