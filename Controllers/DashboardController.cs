using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibrarySystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staffBranch = await _context.StaffBranches
                .FirstOrDefaultAsync(sb => sb.UserId == userId);

            if (staffBranch != null)
            {
                var hasPendingTransfers = await _context.ItemTransfers
                    .AnyAsync(t =>
                        (t.FromBranchId == staffBranch.BranchId && t.Status == TransferStatus.Pending) ||
                        (t.ToBranchId == staffBranch.BranchId && t.Status == TransferStatus.InTransit));

                ViewBag.HasPendingTransfers = hasPendingTransfers;
            }

            return View();
        }
    }
}
