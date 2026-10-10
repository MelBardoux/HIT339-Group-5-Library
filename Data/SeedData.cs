using Microsoft.AspNetCore.Identity;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

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

            // Remove old global Reception and Manager accounts
            var oldGlobalAccounts = new[]
            {
    "reception@library.com",
    "manager@library.com"
};

            foreach (var email in oldGlobalAccounts)
            {
                var oldUser = await userManager.FindByEmailAsync(email);

                if (oldUser != null)
                {
                    var deleteResult = await userManager.DeleteAsync(oldUser);

                    if (!deleteResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to remove old global account: {email}");
                    }
                }
            }

            // Seed branch-specific staff accounts
            var branchStaff = new[]
            {
                new
                {
                    Email = "darwin.receptionists@email.com",
                    Password = "Reception123!",
                    Role = "Reception",
                    BranchName = "Darwin"
                },
                new
                {
                    Email = "darwin.manager@email.com",
                    Password = "Manager123!",
                    Role = "Manager",
                    BranchName = "Darwin"
                },
                new
                {
                    Email = "sydney.receptionists@email.com",
                    Password = "Reception123!",
                    Role = "Reception",
                    BranchName = "Sydney"
                },
                new
                {
                    Email = "sydney.manager@email.com",
                    Password = "Manager123!",
                    Role = "Manager",
                    BranchName = "Sydney"
                },
                new
                {
                    Email = "brisbane.receptionists@email.com",
                    Password = "Reception123!",
                    Role = "Reception",
                    BranchName = "Brisbane"
                },
                new
                {
                    Email = "brisbane.manager@email.com",
                    Password = "Manager123!",
                    Role = "Manager",
                    BranchName = "Brisbane"
                }
            };

            foreach (var staff in branchStaff)
            {
                var user = await userManager.FindByEmailAsync(staff.Email);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = staff.Email,
                        Email = staff.Email,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, staff.Password);

                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create branch account: {staff.Email}");
                    }
                }

                if (!await userManager.IsInRoleAsync(user, staff.Role))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, staff.Role);

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to assign role {staff.Role} to {staff.Email}");
                    }
                }

                var branch = await context.Branches
                    .FirstOrDefaultAsync(b => b.Name == staff.BranchName);

                if (branch == null)
                {
                    throw new InvalidOperationException(
                        $"Branch '{staff.BranchName}' was not found.");
                }

                var existingAssignment = await context.StaffBranches
                    .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

                if (existingAssignment == null)
                {
                    context.StaffBranches.Add(new StaffBranch
                    {
                        UserId = user.Id,
                        BranchId = branch.Id
                    });
                }
            }

            await context.SaveChangesAsync();

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

                // Seed Sydney and Brisbane data
                var sydneyBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Sydney");
                var brisbaneBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Brisbane");

                if (sydneyBranch == null || brisbaneBranch == null)
                {
                    throw new InvalidOperationException("Sydney and Brisbane branches must exist before seeding branch data.");
                }

                // Seed Sydney Borrowers
                if (!context.Borrowers.Any(b => b.LibraryCard == "BRW-0007"))
                {
                    context.Borrowers.AddRange(
                        new Borrower
                        {
                            LibraryCard = "BRW-0007",
                            Name = "Liam Nguyen",
                            DateOfBirth = new DateOnly(1992, 5, 10),
                            Email = "liam.nguyen@email.com",
                            Phone = "0478901234",
                            Address = "14 George Street, Sydney",
                            Status = BorrowerStatus.Active
                        },
                        new Borrower
                        {
                            LibraryCard = "BRW-0008",
                            Name = "Olivia Patel",
                            DateOfBirth = new DateOnly(1988, 9, 25),
                            Email = "olivia.patel@email.com",
                            Phone = "0489012345",
                            Address = "67 Pitt Street, Sydney",
                            Status = BorrowerStatus.Active
                        },
                        new Borrower
                        {
                            LibraryCard = "BRW-0009",
                            Name = "Noah Campbell",
                            DateOfBirth = new DateOnly(2005, 2, 14),
                            Email = "noah.campbell@email.com",
                            Phone = "0490123456",
                            Address = "23 Crown Street, Surry Hills",
                            Status = BorrowerStatus.Active
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Brisbane Borrowers
                if (!context.Borrowers.Any(b => b.LibraryCard == "BRW-0010"))
                {
                    context.Borrowers.AddRange(
                        new Borrower
                        {
                            LibraryCard = "BRW-0010",
                            Name = "Charlotte Wu",
                            DateOfBirth = new DateOnly(1996, 4, 8),
                            Email = "charlotte.wu@email.com",
                            Phone = "0401234567",
                            Address = "45 Queen Street, Brisbane",
                            Status = BorrowerStatus.Active
                        },
                        new Borrower
                        {
                            LibraryCard = "BRW-0011",
                            Name = "James O'Brien",
                            DateOfBirth = new DateOnly(1983, 11, 19),
                            Email = "james.obrien@email.com",
                            Phone = "0412345670",
                            Address = "12 Adelaide Street, Brisbane",
                            Status = BorrowerStatus.Active
                        },
                        new Borrower
                        {
                            LibraryCard = "BRW-0012",
                            Name = "Amara Singh",
                            DateOfBirth = new DateOnly(2001, 7, 30),
                            Email = "amara.singh@email.com",
                            Phone = "0423456701",
                            Address = "88 Boundary Street, West End",
                            Status = BorrowerStatus.Suspended
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Sydney Books
                if (!context.Books.Any(b => b.LibraryCode == "BKS-0011"))
                {
                    var authors = context.Authors.ToList();
                    var genres = context.BookGenres.ToList();

                    context.Books.AddRange(
                        new Book
                        {
                            Name = "The Lord of the Rings",
                            Description = "An epic high-fantasy novel following the quest to destroy the One Ring.",
                            LibraryCode = "BKS-0011",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)),
                            PublicationYear = 1954,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "J.R.R. Tolkien").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Fantasy") }
                        },
                        new Book
                        {
                            Name = "And Then There Were None",
                            Description = "A mystery novel about ten strangers lured to an island.",
                            LibraryCode = "BKS-0012",
                            Status = ItemStatus.Borrowed,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-35)),
                            PublicationYear = 1939,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Agatha Christie").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Mystery") }
                        },
                        new Book
                        {
                            Name = "Sense and Sensibility",
                            Description = "A novel about the Dashwood sisters navigating love and society.",
                            LibraryCode = "BKS-0013",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                            PublicationYear = 1811,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Jane Austen").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Romance") }
                        },
                        new Book
                        {
                            Name = "Diary of a Wimpy Kid: Rodrick Rules",
                            Description = "The second book in the Diary of a Wimpy Kid series.",
                            LibraryCode = "BKS-0014",
                            Status = ItemStatus.Borrowed,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                            PublicationYear = 2008,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Jeff Kinney").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Children's") }
                        },
                        new Book
                        {
                            Name = "Dune Messiah",
                            Description = "The second novel in the Dune saga.",
                            LibraryCode = "BKS-0015",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                            PublicationYear = 1969,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Frank Herbert").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Science Fiction") }
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Sydney Toys
                if (!context.Toys.Any(t => t.LibraryCode == "TOY-0012"))
                {
                    var toyTypes = context.ToyTypes.ToList();

                    context.Toys.AddRange(
                        new Toy
                        {
                            Name = "Chess Set",
                            Description = "A classic wooden chess set for two players.",
                            LibraryCode = "TOY-0012",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                            MinimumAge = 6,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Board Game") }
                        },
                        new Toy
                        {
                            Name = "LEGO Technic Crane",
                            Description = "An advanced building set featuring a working crane mechanism.",
                            LibraryCode = "TOY-0013",
                            Status = ItemStatus.Borrowed,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                            MinimumAge = 10,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Building Set") }
                        },
                        new Toy
                        {
                            Name = "Drone Explorer",
                            Description = "A beginner-friendly drone with camera.",
                            LibraryCode = "TOY-0014",
                            Status = ItemStatus.Damaged,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                            MinimumAge = 12,
                            BatteryRequired = true,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Electronic"), toyTypes.First(t => t.Name == "Outdoor") }
                        },
                        new Toy
                        {
                            Name = "Jigsaw Puzzle 1000pc",
                            Description = "A 1000-piece jigsaw puzzle of the Sydney Opera House.",
                            LibraryCode = "TOY-0015",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                            MinimumAge = 8,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Puzzle") }
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Sydney Music
                if (!context.Music.Any(m => m.LibraryCode == "MUS-0010"))
                {
                    var artists = context.Artists.ToList();
                    var musicGenres = context.MusicGenres.ToList();
                    var formats = context.MusicFormats.ToList();

                    context.Music.AddRange(
                        new Music
                        {
                            Name = "Let It Be",
                            Description = "The twelfth and final studio album by The Beatles.",
                            LibraryCode = "MUS-0010",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
                            AlbumTitle = "Let It Be",
                            ReleaseYear = 1970,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "The Beatles") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Rock") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "Vinyl"), formats.First(f => f.Name == "CD") }
                        },
                        new Music
                        {
                            Name = "Bad",
                            Description = "The seventh studio album by Michael Jackson.",
                            LibraryCode = "MUS-0011",
                            Status = ItemStatus.Borrowed,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                            AlbumTitle = "Bad",
                            ReleaseYear = 1987,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "Michael Jackson") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Pop") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "CD") }
                        },
                        new Music
                        {
                            Name = "Highway to Hell",
                            Description = "The sixth studio album by AC/DC.",
                            LibraryCode = "MUS-0012",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                            AlbumTitle = "Highway to Hell",
                            ReleaseYear = 1979,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "AC/DC") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Rock") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "Vinyl") }
                        },
                        new Music
                        {
                            Name = "Discovery",
                            Description = "The second studio album by Daft Punk.",
                            LibraryCode = "MUS-0013",
                            Status = ItemStatus.Available,
                            BranchId = sydneyBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                            AlbumTitle = "Discovery",
                            ReleaseYear = 2001,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "Daft Punk") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Electronic") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "CD"), formats.First(f => f.Name == "Vinyl") }
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Brisbane Books
                if (!context.Books.Any(b => b.LibraryCode == "BKS-0016"))
                {
                    var authors = context.Authors.ToList();
                    var genres = context.BookGenres.ToList();

                    context.Books.AddRange(
                        new Book
                        {
                            Name = "The Silmarillion",
                            Description = "A collection of mythopoeic works by J.R.R. Tolkien.",
                            LibraryCode = "BKS-0016",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-38)),
                            PublicationYear = 1977,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "J.R.R. Tolkien").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Fantasy") }
                        },
                        new Book
                        {
                            Name = "The ABC Murders",
                            Description = "A Hercule Poirot mystery novel by Agatha Christie.",
                            LibraryCode = "BKS-0017",
                            Status = ItemStatus.Borrowed,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-33)),
                            PublicationYear = 1936,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Agatha Christie").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Mystery") }
                        },
                        new Book
                        {
                            Name = "Emma",
                            Description = "A comic novel about youthful hubris and romantic misunderstandings.",
                            LibraryCode = "BKS-0018",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-28)),
                            PublicationYear = 1815,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Jane Austen").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Romance") }
                        },
                        new Book
                        {
                            Name = "Children of Dune",
                            Description = "The third novel in the Dune saga.",
                            LibraryCode = "BKS-0019",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-23)),
                            PublicationYear = 1976,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Frank Herbert").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Science Fiction") }
                        },
                        new Book
                        {
                            Name = "21 Lessons for the 21st Century",
                            Description = "A book exploring the challenges of the present day.",
                            LibraryCode = "BKS-0020",
                            Status = ItemStatus.Damaged,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-18)),
                            PublicationYear = 2018,
                            PublicationYearUnknown = false,
                            AuthorId = authors.First(a => a.Name == "Yuval Noah Harari").Id,
                            Genres = new List<BookGenre> { genres.First(g => g.Name == "Non-Fiction") }
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Brisbane Toys
                if (!context.Toys.Any(t => t.LibraryCode == "TOY-0016"))
                {
                    var toyTypes = context.ToyTypes.ToList();

                    context.Toys.AddRange(
                        new Toy
                        {
                            Name = "Cluedo",
                            Description = "The classic murder mystery board game.",
                            LibraryCode = "TOY-0016",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-28)),
                            MinimumAge = 8,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Board Game") }
                        },
                        new Toy
                        {
                            Name = "Batman Action Figure",
                            Description = "A poseable Batman figure with cape and accessories.",
                            LibraryCode = "TOY-0017",
                            Status = ItemStatus.Borrowed,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-22)),
                            MinimumAge = 4,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Action Figure") }
                        },
                        new Toy
                        {
                            Name = "Solar System Model Kit",
                            Description = "An educational kit for building a scale model of the solar system.",
                            LibraryCode = "TOY-0018",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-17)),
                            MinimumAge = 8,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Educational") }
                        },
                        new Toy
                        {
                            Name = "Skipping Rope",
                            Description = "An adjustable skipping rope for outdoor play.",
                            LibraryCode = "TOY-0019",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-12)),
                            MinimumAge = 5,
                            BatteryRequired = false,
                            Types = new List<ToyType> { toyTypes.First(t => t.Name == "Outdoor") }
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Brisbane Music
                if (!context.Music.Any(m => m.LibraryCode == "MUS-0014"))
                {
                    var artists = context.Artists.ToList();
                    var musicGenres = context.MusicGenres.ToList();
                    var formats = context.MusicFormats.ToList();

                    context.Music.AddRange(
                        new Music
                        {
                            Name = "Sgt. Pepper's Lonely Hearts Club Band",
                            Description = "The eighth studio album by The Beatles.",
                            LibraryCode = "MUS-0014",
                            Status = ItemStatus.Borrowed,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-28)),
                            AlbumTitle = "Sgt. Pepper's Lonely Hearts Club Band",
                            ReleaseYear = 1967,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "The Beatles") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Rock") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "Vinyl") }
                        },
                        new Music
                        {
                            Name = "9 to 5 and Odd Jobs",
                            Description = "A studio album by Dolly Parton featuring the hit single 9 to 5.",
                            LibraryCode = "MUS-0015",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-22)),
                            AlbumTitle = "9 to 5 and Odd Jobs",
                            ReleaseYear = 1980,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "Dolly Parton") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Country") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "Vinyl"), formats.First(f => f.Name == "Cassette") }
                        },
                        new Music
                        {
                            Name = "Sketches of Spain",
                            Description = "A studio album blending jazz with Spanish folk music by Miles Davis.",
                            LibraryCode = "MUS-0016",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-17)),
                            AlbumTitle = "Sketches of Spain",
                            ReleaseYear = 1960,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "Miles Davis") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "Jazz") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "Vinyl") }
                        },
                        new Music
                        {
                            Name = "Renaissance",
                            Description = "The seventh studio album by Beyoncé.",
                            LibraryCode = "MUS-0017",
                            Status = ItemStatus.Available,
                            BranchId = brisbaneBranch.Id,
                            DateAdded = DateOnly.FromDateTime(DateTime.Now.AddDays(-12)),
                            AlbumTitle = "Renaissance",
                            ReleaseYear = 2022,
                            ReleaseYearUnknown = false,
                            Artists = new List<Artist> { artists.First(a => a.Name == "Beyoncé") },
                            Genres = new List<MusicGenre> { musicGenres.First(g => g.Name == "R&B"), musicGenres.First(g => g.Name == "Electronic") },
                            Formats = new List<MusicFormat> { formats.First(f => f.Name == "CD") }
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Sydney Loans
                if (!context.Loans.Any(l => l.Item.BranchId == sydneyBranch.Id))
                {
                    var sydneyBorrowers = context.Borrowers.Where(b => b.Address.Contains("Sydney") || b.Address.Contains("Surry Hills")).ToList();
                    var sydneyItems = context.Items.Where(i => i.BranchId == sydneyBranch.Id).ToList();

                    var liam = sydneyBorrowers.First(b => b.Name == "Liam Nguyen");
                    var olivia = sydneyBorrowers.First(b => b.Name == "Olivia Patel");
                    var noah = sydneyBorrowers.First(b => b.Name == "Noah Campbell");

                    var andThen = sydneyItems.First(i => i.LibraryCode == "BKS-0012");
                    var rodrick = sydneyItems.First(i => i.LibraryCode == "BKS-0014");
                    var legoTechnic = sydneyItems.First(i => i.LibraryCode == "TOY-0013");
                    var bad = sydneyItems.First(i => i.LibraryCode == "MUS-0011");

                    context.Loans.AddRange(
                        new Loan
                        {
                            ItemId = andThen.Id,
                            BorrowerId = liam.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-6)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(8))
                        },
                        new Loan
                        {
                            ItemId = rodrick.Id,
                            BorrowerId = noah.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-4)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10))
                        },
                        new Loan
                        {
                            ItemId = legoTechnic.Id,
                            BorrowerId = olivia.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(11))
                        },
                        new Loan
                        {
                            ItemId = bad.Id,
                            BorrowerId = liam.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                            Fine = 5.00m
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Brisbane Loans
                if (!context.Loans.Any(l => l.Item.BranchId == brisbaneBranch.Id))
                {
                    var brisbaneBorrowers = context.Borrowers.Where(b => b.Address.Contains("Brisbane") || b.Address.Contains("West End")).ToList();
                    var brisbaneItems = context.Items.Where(i => i.BranchId == brisbaneBranch.Id).ToList();

                    var charlotte = brisbaneBorrowers.First(b => b.Name == "Charlotte Wu");
                    var james = brisbaneBorrowers.First(b => b.Name == "James O'Brien");
                    var amara = brisbaneBorrowers.First(b => b.Name == "Amara Singh");

                    var abcMurders = brisbaneItems.First(i => i.LibraryCode == "BKS-0017");
                    var batman = brisbaneItems.First(i => i.LibraryCode == "TOY-0017");
                    var sgtPepper = brisbaneItems.First(i => i.LibraryCode == "MUS-0014");

                    context.Loans.AddRange(
                        new Loan
                        {
                            ItemId = abcMurders.Id,
                            BorrowerId = james.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(9))
                        },
                        new Loan
                        {
                            ItemId = batman.Id,
                            BorrowerId = charlotte.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(11))
                        },
                        new Loan
                        {
                            ItemId = sgtPepper.Id,
                            BorrowerId = amara.Id,
                            BorrowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-8)),
                            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                            Fine = 3.00m
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Seed Reservations
                if (!context.Reservations.Any())
                {
                    var allBorrowers = context.Borrowers.ToList();
                    var allItems = context.Items.ToList();

                    // Noah in Sydney wants "And Then There Were None" which Liam has borrowed
                    var andThenItem = allItems.First(i => i.LibraryCode == "BKS-0012");
                    var noahBorrower = allBorrowers.First(b => b.Name == "Noah Campbell");

                    // Charlotte in Brisbane wants "The ABC Murders" which James has borrowed
                    var abcItem = allItems.First(i => i.LibraryCode == "BKS-0017");
                    var charlotteBorrower = allBorrowers.First(b => b.Name == "Charlotte Wu");

                    // Olivia in Sydney wants "Thriller" which Alex has in Darwin (cross-branch scenario - item borrowed at Darwin, but Sydney has no copy)
                    var thrillerItem = allItems.First(i => i.LibraryCode == "MUS-0002");
                    var oliviaBorrower = allBorrowers.First(b => b.Name == "Olivia Patel");

                    // Jane in Darwin wants "Dune" which Emily has borrowed (existing Darwin reservation)
                    var duneItem = allItems.First(i => i.LibraryCode == "BKS-0006");
                    var janeBorrower = allBorrowers.First(b => b.Name == "Jane Smith");

                    context.Reservations.AddRange(
                        new Reservation
                        {
                            ItemId = andThenItem.Id,
                            BorrowerId = noahBorrower.Id,
                            PlacedAt = DateTime.Now.AddDays(-2),
                            QueuePosition = 1,
                            Status = ReservationStatus.Waiting
                        },
                        new Reservation
                        {
                            ItemId = abcItem.Id,
                            BorrowerId = charlotteBorrower.Id,
                            PlacedAt = DateTime.Now.AddDays(-1),
                            QueuePosition = 1,
                            Status = ReservationStatus.Waiting
                        },
                        new Reservation
                        {
                            ItemId = thrillerItem.Id,
                            BorrowerId = oliviaBorrower.Id,
                            PlacedAt = DateTime.Now.AddDays(-3),
                            QueuePosition = 1,
                            Status = ReservationStatus.Waiting
                        },
                        new Reservation
                        {
                            ItemId = duneItem.Id,
                            BorrowerId = janeBorrower.Id,
                            PlacedAt = DateTime.Now.AddDays(-4),
                            QueuePosition = 1,
                            Status = ReservationStatus.Waiting
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}