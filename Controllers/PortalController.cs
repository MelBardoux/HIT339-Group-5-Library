using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    public class PortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Portal
        public async Task<IActionResult> Index(string? searchTerm, string? tab, int? genreId, int? typeId, int? formatId, string? sort, int? branchId)
        {
            var viewModel = new PortalViewModel
            {
                SearchTerm = searchTerm,
                ActiveTab = tab ?? "books",
                SelectedGenreId = genreId,
                SelectedTypeId = typeId,
                SelectedFormatId = formatId,
                SelectedSort = sort ?? "name",
                SelectedBranchId = branchId
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                viewModel.SearchResults = await Search(searchTerm);
                viewModel.IsSearching = true;
                return View(viewModel);
            }

            viewModel.BookGenres = await _context.BookGenres.OrderBy(g => g.Name).ToListAsync();
            viewModel.ToyTypes = await _context.ToyTypes.OrderBy(t => t.Name).ToListAsync();
            viewModel.MusicFormats = await _context.MusicFormats.OrderBy(f => f.Name).ToListAsync();
            viewModel.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();

            viewModel.Books = await GetBooks(genreId, sort ?? "name", branchId);
            viewModel.Toys = await GetToys(typeId, sort ?? "name", branchId);
            viewModel.Music = await GetMusic(formatId, sort ?? "name", branchId);

            return View(viewModel);
        }

        // GET: Portal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            if (item is Book)
            {
                var book = await _context.Books
                    .Include(b => b.Author)
                    .Include(b => b.Genres)
                    .FirstOrDefaultAsync(b => b.Id == id);

                return View("BookDetails", new SearchBookDetailsViewModel
                {
                    LibraryCode = book.LibraryCode,
                    Name = book.Name,
                    Description = book.Description,
                    Author = book.Author.Name,
                    PublicationYear = book.PublicationYearUnknown ? "Unknown" : book.PublicationYear?.ToString(),
                    Genres = book.Genres.Select(g => g.Name).ToList(),
                    OtherGenre = book.OtherGenre ? book.OtherGenreText : null,
                    Status = book.Status.ToString()
                });
            }

            if (item is Toy)
            {
                var toy = await _context.Toys
                    .Include(t => t.Types)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return View("ToyDetails", new SearchToyDetailsViewModel
                {
                    LibraryCode = toy.LibraryCode,
                    Name = toy.Name,
                    Description = toy.Description,
                    AgeDisplay = toy.AgeDisplay,
                    BatteryRequired = toy.BatteryRequired,
                    Types = toy.Types.Select(t => t.Name).ToList(),
                    Status = toy.Status.ToString()
                });
            }

            if (item is Music)
            {
                var musicItem = await _context.Music
                    .Include(m => m.Artists)
                    .Include(m => m.Genres)
                    .Include(m => m.Formats)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return View("MusicDetails", new SearchMusicDetailsViewModel
                {
                    LibraryCode = musicItem.LibraryCode,
                    Name = musicItem.Name,
                    Description = musicItem.Description,
                    AlbumTitle = musicItem.AlbumTitle,
                    ReleaseYear = musicItem.ReleaseYearUnknown ? "Unknown" : musicItem.ReleaseYear?.ToString(),
                    Artists = musicItem.Artists.Select(a => a.Name).ToList(),
                    Genres = musicItem.Genres.Select(g => g.Name).ToList(),
                    Formats = musicItem.Formats.Select(f => f.Name).ToList(),
                    Status = musicItem.Status.ToString()
                });
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var t = term.Trim().ToLower();

            var items = await _context.Items
                .Where(i => i.Status != ItemStatus.Destroyed && i.Status != ItemStatus.Lost)
                .Where(i => i.Name.ToLower().Contains(t) || i.LibraryCode.ToLower().Contains(t))
                .Take(10)
                .Select(i => new { id = i.Id, text = i.Name + " (" + i.LibraryCode + ")" })
                .ToListAsync();

            return Json(items);
        }

        // GET: Portal/Random/book
        public async Task<IActionResult> Random(string type)
        {
            var query = type?.ToLower() switch
            {
                "book" => _context.Books.Where(b => b.Status != ItemStatus.Destroyed && b.Status != ItemStatus.Lost).Cast<Item>(),
                "toy" => _context.Toys.Where(t => t.Status != ItemStatus.Destroyed && t.Status != ItemStatus.Lost).Cast<Item>(),
                "music" => _context.Music.Where(m => m.Status != ItemStatus.Destroyed && m.Status != ItemStatus.Lost).Cast<Item>(),
                _ => null
            };

            if (query == null) return RedirectToAction(nameof(Index));

            var count = await query.CountAsync();
            if (count == 0) return RedirectToAction(nameof(Index));

            var random = new Random();
            var item = await query.Skip(random.Next(count)).FirstAsync();

            return RedirectToAction(nameof(Details), new { id = item.Id });
        }

        private async Task<List<SearchResultViewModel>> Search(string searchTerm)
        {
            var term = searchTerm.Trim().ToLower();

            var books = await _context.Books
    .Include(b => b.Author)
    .Include(b => b.Genres)
    .Include(b => b.Branch)
                .Where(b => b.Status != ItemStatus.Destroyed && b.Status != ItemStatus.Lost)
                .Where(b =>
                    b.Name.ToLower().Contains(term) ||
                    b.Description.ToLower().Contains(term) ||
                    b.LibraryCode.ToLower().Contains(term) ||
                    b.Author.Name.ToLower().Contains(term) ||
                    b.Genres.Any(g => g.Name.ToLower().Contains(term)))
                .Select(b => new SearchResultViewModel
                {
                    Id = b.Id,
                    LibraryCode = b.LibraryCode,
                    Name = b.Name,
                    Type = "Book",
                    Status = b.Status.ToString(),
                    Summary = "By " + b.Author.Name + (b.PublicationYear != null ? " (" + b.PublicationYear + ")" : ""),
                    BranchName = b.Branch.Name
                })
                .ToListAsync();

            var toys = await _context.Toys
                .Include(t => t.Types)
                .Include(t => t.Branch)
                .Where(t => t.Status != ItemStatus.Destroyed && t.Status != ItemStatus.Lost)
                .Where(t =>
                    t.Name.ToLower().Contains(term) ||
                    t.Description.ToLower().Contains(term) ||
                    t.LibraryCode.ToLower().Contains(term) ||
                    t.Types.Any(tt => tt.Name.ToLower().Contains(term)))
                .Select(t => new SearchResultViewModel
                {
                    Id = t.Id,
                    LibraryCode = t.LibraryCode,
                    Name = t.Name,
                    Type = "Toy",
                    Status = t.Status.ToString(),
                    Summary = "Ages " + t.MinimumAge + "+",
                    BranchName = t.Branch.Name
                })
                .ToListAsync();

            var music = await _context.Music
    .Include(m => m.Artists)
    .Include(m => m.Genres)
    .Include(m => m.Branch)
                .Where(m => m.Status != ItemStatus.Destroyed && m.Status != ItemStatus.Lost)
                .Where(m =>
                    m.Name.ToLower().Contains(term) ||
                    m.Description.ToLower().Contains(term) ||
                    m.LibraryCode.ToLower().Contains(term) ||
                    m.AlbumTitle.ToLower().Contains(term) ||
                    m.Artists.Any(a => a.Name.ToLower().Contains(term)) ||
                    m.Genres.Any(g => g.Name.ToLower().Contains(term)))
                .Select(m => new SearchResultViewModel
                {
                    Id = m.Id,
                    LibraryCode = m.LibraryCode,
                    Name = m.Name,
                    Type = "Music",
                    Status = m.Status.ToString(),
                    Summary = m.AlbumTitle + " - " + string.Join(", ", m.Artists.Select(a => a.Name)),
                    BranchName = m.Branch.Name
                })
                .ToListAsync();

            return books.Concat(toys).Concat(music).OrderBy(r => r.Name).ToList();
        }

        private async Task<List<PortalBookViewModel>> GetBooks(int? genreId, string sort, int? branchId)
        {
            var query = _context.Books
    .Include(b => b.Author)
    .Include(b => b.Genres)
    .Include(b => b.Branch)
                .Where(b => b.Status != ItemStatus.Destroyed && b.Status != ItemStatus.Lost)
                .AsQueryable();

            if (genreId.HasValue)
                query = query.Where(b => b.Genres.Any(g => g.Id == genreId));
            if (branchId.HasValue)
                query = query.Where(b => b.BranchId == branchId.Value);

            var books = await query.Select(b => new PortalBookViewModel
            {
                Id = b.Id,
                LibraryCode = b.LibraryCode,
                Name = b.Name,
                AuthorName = b.Author.Name,
                PublicationYear = b.PublicationYearUnknown ? "Unknown" : b.PublicationYear.ToString(),
                Status = b.Status.ToString(),
                BranchName = b.Branch.Name,
                Genres = b.Genres.Select(g => g.Name).ToList()
            }).ToListAsync();

            return sort switch
            {
                "author" => books.OrderBy(b => b.AuthorName).ToList(),
                "year" => books.OrderBy(b => b.PublicationYear).ToList(),
                _ => books.OrderBy(b => b.Name).ToList()
            };
        }

        private async Task<List<PortalToyViewModel>> GetToys(int? typeId, string sort, int? branchId)
        {
            var query = _context.Toys
    .Include(t => t.Types)
    .Include(t => t.Branch)
                .Where(t => t.Status != ItemStatus.Destroyed && t.Status != ItemStatus.Lost)
                .AsQueryable();

            if (typeId.HasValue)
                query = query.Where(t => t.Types.Any(tt => tt.Id == typeId));

            if (branchId.HasValue)
                query = query.Where(t => t.BranchId == branchId.Value);

            var toys = await query.Select(t => new PortalToyViewModel
            {
                Id = t.Id,
                LibraryCode = t.LibraryCode,
                Name = t.Name,
                AgeDisplay = t.AgeDisplay,
                BatteryRequired = t.BatteryRequired,
                Status = t.Status.ToString(),
                BranchName = t.Branch.Name,
                Types = t.Types.Select(tt => tt.Name).ToList()
            }).ToListAsync();

            return sort switch
            {
                "age" => toys.OrderBy(t => t.AgeDisplay).ToList(),
                _ => toys.OrderBy(t => t.Name).ToList()
            };
        }

        private async Task<List<PortalMusicViewModel>> GetMusic(int? formatId, string sort, int? branchId)
        {
            var query = _context.Music
    .Include(m => m.Artists)
    .Include(m => m.Genres)
    .Include(m => m.Formats)
    .Include(m => m.Branch)
                .Where(m => m.Status != ItemStatus.Destroyed && m.Status != ItemStatus.Lost)
                .AsQueryable();

            if (formatId.HasValue)
                query = query.Where(m => m.Formats.Any(f => f.Id == formatId));

            if (branchId.HasValue)
                query = query.Where(m => m.BranchId == branchId.Value);

            var musicItems = await query.Select(m => new PortalMusicViewModel
            {
                Id = m.Id,
                LibraryCode = m.LibraryCode,
                Name = m.Name,
                AlbumTitle = m.AlbumTitle,
                Artists = string.Join(", ", m.Artists.Select(a => a.Name)),
                ReleaseYear = m.ReleaseYearUnknown ? "Unknown" : m.ReleaseYear.ToString(),
                Status = m.Status.ToString(),
                BranchName = m.Branch.Name,
                Formats = m.Formats.Select(f => f.Name).ToList(),
                Genres = m.Genres.Select(g => g.Name).ToList()
            }).ToListAsync();

            return sort switch
            {
                "artist" => musicItems.OrderBy(m => m.Artists).ToList(),
                "year" => musicItems.OrderBy(m => m.ReleaseYear).ToList(),
                "genre" => musicItems.OrderBy(m => string.Join(", ", m.Genres)).ToList(),
                _ => musicItems.OrderBy(m => m.Name).ToList()
            };
        }
    }
}