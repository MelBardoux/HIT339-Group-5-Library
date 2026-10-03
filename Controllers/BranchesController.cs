using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Reception,Manager")] // Design choice to only allow Reception and Manager (STAFF)
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Branches/Inventory
        public async Task<IActionResult> Inventory(int? branchId)
        {
            var branches = await _context.Branches // Gets the three branches
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .ToListAsync();

            var query = _context.Items // Gets the library inventory and its associated branch
                .Include(i => i.Branch)
                .AsNoTracking()
                .AsQueryable();

            if (branchId.HasValue) // branch filtering option - besically, nothing selected = show all, 1 = darwin, 2 = sydney, 3 = brisbane (Inventory)
            {
                query = query.Where(i => i.BranchId == branchId.Value);
            }

            var items = await query
                .OrderBy(i => i.Name)
                .ToListAsync();

            var viewModel = new BranchInventoryViewModel
            {
                SelectedBranchId = branchId,
                Branches = branches,
                Items = items.Select(i => new BranchInventoryItemViewModel
                {
                    Id = i.Id,
                    LibraryCode = i.LibraryCode,
                    Name = i.Name,
                    Type = i.GetType().Name,
                    Status = i.Status.ToString(),
                    BranchName = i.Branch.Name,
                    OnOrder = i.OnOrder
                }).ToList()
            };

            return View(viewModel);
        }
    }
}