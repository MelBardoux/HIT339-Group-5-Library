using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new BookViewModel();
            await viewModel.LoadGenres(_context);
            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var book = await viewModel.ToBook(_context);
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await viewModel.ReloadGenres(_context);
            return View(viewModel);
        }

        // GET: Books/Index
        public async Task<IActionResult> Index(string? searchTerm, string? status)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.ReservedForBorrower)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(term) ||
                    b.LibraryCode.ToLower().Contains(term) ||
                    b.Author.Name.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "HasReservation")
                {
                    query = query.Where(b => b.ReservedForBorrowerId != null);
                }
                else if (status == "OnOrder")
                {
                    query = query.Where(b => b.OnOrder);
                }
                else if (Enum.TryParse<ItemStatus>(status, out var statusEnum))
                {
                    query = query.Where(b => b.Status == statusEnum);
                }
            }

            var books = await query
                .Select(b => new BookIndexViewModel
                {
                    Id = b.Id,
                    LibraryCode = b.LibraryCode,
                    Name = b.Name,
                    AuthorName = b.Author.Name,
                    PublicationYear = b.PublicationYear,
                    PublicationYearUnknown = b.PublicationYearUnknown,
                    Status = b.Status.ToString(),
                    OnOrder = b.OnOrder,
                    ReservedForCard = b.ReservedForBorrower != null ? b.ReservedForBorrower.LibraryCard : null
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;

            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookDetailsViewModel
            {
                Id = book.Id,
                LibraryCode = book.LibraryCode,
                Name = book.Name,
                Description = book.Description,
                AuthorName = book.Author.Name,
                PublicationYear = book.PublicationYear,
                PublicationYearUnknown = book.PublicationYearUnknown,
                Genres = book.Genres.Select(g => g.Name).ToList(),
                OtherGenre = book.OtherGenre,
                OtherGenreText = book.OtherGenreText,
                Status = book.Status.ToString(),
                DateAdded = book.DateAdded
            };

            return View(viewModel);
        }

        // GET: Books/Delete/1
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookDeleteViewModel
            {
                Id = book.Id,
                LibraryCode = book.LibraryCode,
                Name = book.Name,
                AuthorName = book.Author.Name
            };

            return View(viewModel);
        }

        // POST: Books/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookEditViewModel
            {
                Id = book.Id,
                LibraryCode = book.LibraryCode,
                Name = book.Name,
                Description = book.Description,
                PublicationYear = book.PublicationYear,
                PublicationYearUnknown = book.PublicationYearUnknown,
                AuthorName = book.Author.Name,
                AuthorId = book.AuthorId,
                Status = book.Status,
                OtherGenreSelected = book.OtherGenre,
                OtherGenreName = book.OtherGenreText,
                OnOrder = book.OnOrder,
            };

            await viewModel.LoadGenres(_context, book.Genres);
            return View(viewModel);
        }

        // POST: Books/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var book = await _context.Books
                    .Include(b => b.Genres)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (book == null)
                {
                    return NotFound();
                }

                await viewModel.UpdateBook(_context, book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await viewModel.ReloadGenres(_context);
            return View(viewModel);
        }

        // GET: Books/SearchBooks
        [HttpGet]
        public async Task<IActionResult> SearchBooks(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var t = term.Trim().ToLower();

            var books = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.Name.ToLower().Contains(t) ||
                             b.LibraryCode.ToLower().Contains(t) ||
                             b.Author.Name.ToLower().Contains(t))
                .Take(10)
                .Select(b => new { id = b.Name, text = b.Name + " (" + b.LibraryCode + ")" })
                .ToListAsync();

            return Json(books);
        }
    }
}