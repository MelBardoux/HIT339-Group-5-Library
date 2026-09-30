using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Reception")]
    public class BorrowersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Borrowers
        public async Task<IActionResult> Index(string? searchTerm, string? status)
        {
            var query = _context.Borrowers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(term) ||
                    b.LibraryCard.ToLower().Contains(term) ||
                    b.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BorrowerStatus>(status, out var statusEnum))
            {
                query = query.Where(b => b.Status == statusEnum);
            }

            var borrowers = await query
                .Select(b => new BorrowerIndexViewModel
                {
                    Id = b.Id,
                    LibraryCard = b.LibraryCard,
                    Name = b.Name,
                    Email = b.Email,
                    Phone = b.Phone,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;

            return View(borrowers);
        }

        // GET: Borrowers/Create
        public IActionResult Create()
        {
            return View(new BorrowerViewModel());
        }

        // POST: Borrowers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowerViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var borrower = await viewModel.ToBorrower(_context);
                _context.Borrowers.Add(borrower);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Borrowers/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrower == null)
            {
                return NotFound();
            }

            var viewModel = new BorrowerDetailsViewModel
            {
                Id = borrower.Id,
                LibraryCard = borrower.LibraryCard,
                Name = borrower.Name,
                DateOfBirth = borrower.DateOfBirth,
                Email = borrower.Email,
                Phone = borrower.Phone,
                Address = borrower.Address,
                Status = borrower.Status.ToString()
            };

            return View(viewModel);
        }

        // GET: Borrowers/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrower == null)
            {
                return NotFound();
            }

            var viewModel = new BorrowerEditViewModel
            {
                Id = borrower.Id,
                LibraryCard = borrower.LibraryCard,
                Name = borrower.Name,
                DateOfBirth = borrower.DateOfBirth,
                Email = borrower.Email,
                Phone = borrower.Phone,
                Address = borrower.Address,
                Status = borrower.Status
            };

            return View(viewModel);
        }

        // POST: Borrowers/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BorrowerEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var borrower = await _context.Borrowers
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (borrower == null)
                {
                    return NotFound();
                }

                borrower.Name = viewModel.Name;
                borrower.DateOfBirth = viewModel.DateOfBirth.Value;
                borrower.Email = viewModel.Email;
                borrower.Phone = viewModel.Phone;
                borrower.Address = viewModel.Address;
                borrower.Status = viewModel.Status;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Borrowers/Delete/1
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrower = await _context.Borrowers
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrower == null)
            {
                return NotFound();
            }

            var viewModel = new BorrowerDeleteViewModel
            {
                Id = borrower.Id,
                LibraryCard = borrower.LibraryCard,
                Name = borrower.Name
            };

            return View(viewModel);
        }

        // POST: Borrowers/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrower = await _context.Borrowers.FindAsync(id);
            if (borrower != null)
            {
                _context.Borrowers.Remove(borrower);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Borrowers/SearchBorrowers
        [HttpGet]
        public async Task<IActionResult> SearchBorrowers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var t = term.Trim().ToLower();

            var borrowers = await _context.Borrowers
                .Where(b => b.Name.ToLower().Contains(t) ||
                             b.LibraryCard.ToLower().Contains(t))
                .Take(10)
                .Select(b => new { id = b.Name, text = b.Name + " (" + b.LibraryCard + ")" })
                .ToListAsync();

            return Json(borrowers);
        }
    }
}