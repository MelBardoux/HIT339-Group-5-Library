using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ToysController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToysController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Toys/Index
        public async Task<IActionResult> Index(string? searchTerm, string? status)
        {
            var query = _context.Toys
                .Include(t => t.Types)
                .Include(t => t.ReservedForBorrower)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(term) ||
                    t.LibraryCode.ToLower().Contains(term) ||
                    t.Types.Any(tt => tt.Name.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "HasReservation")
                {
                    query = query.Where(t => t.ReservedForBorrowerId != null);
                }
                else if (status == "OnOrder")
                {
                    query = query.Where(t => t.OnOrder);
                }
                else if (Enum.TryParse<ItemStatus>(status, out var statusEnum))
                {
                    query = query.Where(t => t.Status == statusEnum);
                }
            }

            var toys = await query
                .Select(t => new ToyIndexViewModel
                {
                    Id = t.Id,
                    LibraryCode = t.LibraryCode,
                    Name = t.Name,
                    AgeDisplay = t.AgeDisplay,
                    BatteryRequired = t.BatteryRequired,
                    Status = t.Status.ToString(),
                    OnOrder = t.OnOrder,
                    ReservedForCard = t.ReservedForBorrower != null ? t.ReservedForBorrower.LibraryCard : null
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;

            return View(toys);
        }

        // GET: Toys/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ToyViewModel();
            await viewModel.LoadTypes(_context);
            return View(viewModel);
        }

        // POST: Toys/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ToyViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var toy = await viewModel.ToToy(_context);
                _context.Toys.Add(toy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await viewModel.ReloadTypes(_context);
            return View(viewModel);
        }

        // GET: Toys/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toy = await _context.Toys
                .Include(t => t.Types)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (toy == null)
            {
                return NotFound();
            }

            var viewModel = new ToyDetailsViewModel
            {
                Id = toy.Id,
                LibraryCode = toy.LibraryCode,
                Name = toy.Name,
                Description = toy.Description,
                AgeDisplay = toy.AgeDisplay,
                BatteryRequired = toy.BatteryRequired,
                Types = toy.Types.Select(t => t.Name).ToList(),
                Status = toy.Status.ToString(),
                DateAdded = toy.DateAdded
            };

            return View(viewModel);
        }

        // GET: Toys/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toy = await _context.Toys
                .Include(t => t.Types)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (toy == null)
            {
                return NotFound();
            }

            var viewModel = new ToyEditViewModel
            {
                Id = toy.Id,
                LibraryCode = toy.LibraryCode,
                Name = toy.Name,
                Description = toy.Description,
                MinimumAge = toy.MinimumAge,
                BatteryRequired = toy.BatteryRequired,
                Status = toy.Status,
                OnOrder = toy.OnOrder
            };

            await viewModel.LoadTypes(_context, toy.Types);
            return View(viewModel);
        }

        // POST: Toys/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ToyEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var toy = await _context.Toys
                    .Include(t => t.Types)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (toy == null)
                {
                    return NotFound();
                }

                await viewModel.UpdateToy(_context, toy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await viewModel.ReloadTypes(_context);
            return View(viewModel);
        }

        // GET: Toys/Delete/1
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toy = await _context.Toys
                .FirstOrDefaultAsync(t => t.Id == id);

            if (toy == null)
            {
                return NotFound();
            }

            var viewModel = new ToyDeleteViewModel
            {
                Id = toy.Id,
                LibraryCode = toy.LibraryCode,
                Name = toy.Name,
                AgeDisplay = toy.AgeDisplay
            };

            return View(viewModel);
        }

        // POST: Toys/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var toy = await _context.Toys.FindAsync(id);
            if (toy != null)
            {
                _context.Toys.Remove(toy);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Toys/SearchToys
        [HttpGet]
        public async Task<IActionResult> SearchToys(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var t = term.Trim().ToLower();

            var toys = await _context.Toys
                .Where(t2 => t2.Name.ToLower().Contains(t) ||
                              t2.LibraryCode.ToLower().Contains(t))
                .Take(10)
                .Select(t2 => new { id = t2.Name, text = t2.Name + " (" + t2.LibraryCode + ")" })
                .ToListAsync();

            return Json(toys);
        }
    }
}