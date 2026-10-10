using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Reception,Manager")]
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Branches/Inventory
        public async Task<IActionResult> Inventory(int? branchId, string? type)
        {
            var branches = await _context.Branches
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .ToListAsync();

            var query = _context.Items
                .Include(i => i.Branch)
                .AsNoTracking()
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(i => i.BranchId == branchId.Value);
            }

            var allItems = await query
                .OrderBy(i => i.Name)
                .ToListAsync();

            if (!string.IsNullOrEmpty(type))
            {
                allItems = allItems.Where(i => i.GetType().Name == type).ToList();
            }

            var viewModel = new BranchInventoryViewModel
            {
                SelectedBranchId = branchId,
                SelectedType = type,
                Branches = branches,
                Items = allItems.Select(i => new BranchInventoryItemViewModel
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