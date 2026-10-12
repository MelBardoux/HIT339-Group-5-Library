using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace LibrarySystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var staffBranch = await _context.StaffBranches
                    .FirstOrDefaultAsync(sb => sb.UserId == userId);

                if (staffBranch != null)
                {
                    // Check for transfers requiring action at this branch
                    var hasPendingTransfers = await _context.ItemTransfers
                        .AnyAsync(t =>
                            (t.FromBranchId == staffBranch.BranchId && t.Status == TransferStatus.Pending) ||
                            (t.ToBranchId == staffBranch.BranchId && t.Status == TransferStatus.InTransit));

                    ViewBag.HasPendingTransfers = hasPendingTransfers;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}