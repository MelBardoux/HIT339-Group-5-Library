using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MusicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MusicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Music/Index
        public async Task<IActionResult> Index(string? searchTerm, string? status)
        {
            var query = _context.Music
                .Include(m => m.Artists)
                .Include(m => m.ReservedForBorrower)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(term) ||
                    m.LibraryCode.ToLower().Contains(term) ||
                    m.AlbumTitle.ToLower().Contains(term) ||
                    m.Artists.Any(a => a.Name.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "HasReservation")
                {
                    query = query.Where(m => m.ReservedForBorrowerId != null);
                }
                else if (status == "OnOrder")
                {
                    query = query.Where(m => m.OnOrder);
                }
                else if (Enum.TryParse<ItemStatus>(status, out var statusEnum))
                {
                    query = query.Where(m => m.Status == statusEnum);
                }
            }

            var music = await query
                .Select(m => new MusicIndexViewModel
                {
                    Id = m.Id,
                    LibraryCode = m.LibraryCode,
                    Name = m.Name,
                    AlbumTitle = m.AlbumTitle,
                    Artists = string.Join(", ", m.Artists.Select(a => a.Name)),
                    ReleaseYear = m.ReleaseYear,
                    ReleaseYearUnknown = m.ReleaseYearUnknown,
                    Status = m.Status.ToString(),
                    OnOrder = m.OnOrder,
                    ReservedForCard = m.ReservedForBorrower != null ? m.ReservedForBorrower.LibraryCard : null
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;

            return View(music);
        }

        // GET: Music/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new MusicViewModel();
            await viewModel.LoadGenresAndFormats(_context);
            return View(viewModel);
        }

        // POST: Music/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MusicViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var music = await viewModel.ToMusic(_context);
                _context.Music.Add(music);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await viewModel.ReloadGenresAndFormats(_context);
            return View(viewModel);
        }

        // GET: Music/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var music = await _context.Music
                .Include(m => m.Artists)
                .Include(m => m.Genres)
                .Include(m => m.Formats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (music == null)
            {
                return NotFound();
            }

            var viewModel = new MusicDetailsViewModel
            {
                Id = music.Id,
                LibraryCode = music.LibraryCode,
                Name = music.Name,
                Description = music.Description,
                AlbumTitle = music.AlbumTitle,
                ReleaseYear = music.ReleaseYear,
                ReleaseYearUnknown = music.ReleaseYearUnknown,
                Artists = music.Artists.Select(a => a.Name).ToList(),
                Genres = music.Genres.Select(g => g.Name).ToList(),
                Formats = music.Formats.Select(f => f.Name).ToList(),
                Status = music.Status.ToString(),
                DateAdded = music.DateAdded
            };

            return View(viewModel);
        }

        // GET: Music/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var music = await _context.Music
                .Include(m => m.Artists)
                .Include(m => m.Genres)
                .Include(m => m.Formats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (music == null)
            {
                return NotFound();
            }

            var viewModel = new MusicEditViewModel
            {
                Id = music.Id,
                LibraryCode = music.LibraryCode,
                Name = music.Name,
                Description = music.Description,
                AlbumTitle = music.AlbumTitle,
                ReleaseYear = music.ReleaseYear,
                ReleaseYearUnknown = music.ReleaseYearUnknown,
                Status = music.Status,
                ArtistNames = music.Artists.Select(a => a.Name).ToList(),
                OnOrder = music.OnOrder
            };

            await viewModel.LoadGenresAndFormats(_context, music.Genres, music.Formats);
            return View(viewModel);
        }

        // POST: Music/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MusicEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var music = await _context.Music
                    .Include(m => m.Artists)
                    .Include(m => m.Genres)
                    .Include(m => m.Formats)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (music == null)
                {
                    return NotFound();
                }

                await viewModel.UpdateMusic(_context, music);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await viewModel.ReloadGenresAndFormats(_context);
            return View(viewModel);
        }

        // GET: Music/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var music = await _context.Music
                .FirstOrDefaultAsync(m => m.Id == id);

            if (music == null)
            {
                return NotFound();
            }

            var viewModel = new MusicDeleteViewModel
            {
                Id = music.Id,
                LibraryCode = music.LibraryCode,
                Name = music.Name,
                AlbumTitle = music.AlbumTitle
            };

            return View(viewModel);
        }

        // POST: Music/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var music = await _context.Music.FindAsync(id);
            if (music != null)
            {
                _context.Music.Remove(music);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Music/SearchMusic
        [HttpGet]
        public async Task<IActionResult> SearchMusic(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var t = term.Trim().ToLower();

            var music = await _context.Music
                .Include(m => m.Artists)
                .Where(m => m.Name.ToLower().Contains(t) ||
                             m.LibraryCode.ToLower().Contains(t) ||
                             m.AlbumTitle.ToLower().Contains(t) ||
                             m.Artists.Any(a => a.Name.ToLower().Contains(t)))
                .Take(10)
                .Select(m => new { id = m.Name, text = m.Name + " (" + m.LibraryCode + ")" })
                .ToListAsync();

            return Json(music);
        }
    }
}