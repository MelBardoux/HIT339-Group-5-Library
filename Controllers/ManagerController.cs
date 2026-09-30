using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Manager
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var viewModel = new ManagerViewModel
            {
                BorrowingStats = await GetBorrowingStats(today),
                ItemStats = await GetItemStats(),
                FineStats = await GetFineStats()
            };

            return View(viewModel);
        }

        private async Task<BorrowingStatsViewModel> GetBorrowingStats(DateOnly today)
        {
            var stats = new BorrowingStatsViewModel
            {
                TotalLoans = await _context.Loans.CountAsync(),
                ActiveLoans = await _context.Loans.CountAsync(l => l.ReturnedDate == null),
                OverdueLoans = await _context.Loans.CountAsync(l => l.ReturnedDate == null && l.DueDate < today),

                MostBorrowedItems = await _context.Loans
                    .Include(l => l.Item)
                    .GroupBy(l => l.ItemId)
                    .Select(g => new MostBorrowedItemViewModel
                    {
                        LibraryCode = g.First().Item.LibraryCode,
                        Name = g.First().Item.Name,
                        Type = g.First().Item.GetType().Name,
                        BorrowCount = g.Count()
                    })
                    .OrderByDescending(x => x.BorrowCount)
                    .Take(5)
                    .ToListAsync(),

                BusiestBorrowers = await _context.Loans
                    .Include(l => l.Borrower)
                    .GroupBy(l => l.BorrowerId)
                    .Select(g => new BusiestBorrowerViewModel
                    {
                        LibraryCard = g.First().Borrower.LibraryCard,
                        Name = g.First().Borrower.Name,
                        LoanCount = g.Count()
                    })
                    .OrderByDescending(x => x.LoanCount)
                    .Take(5)
                    .ToListAsync(),

                LoansPerMonth = (await _context.Loans
                    .GroupBy(l => new { l.BorrowedDate.Year, l.BorrowedDate.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .Take(12)
                    .ToListAsync())
                    .Select(x => new MonthlyCountViewModel
                    {
                        Month = x.Year + "-" + x.Month.ToString("D2"),
                        Count = x.Count
                    })
                    .ToList()
            };

            return stats;
        }

        private async Task<ItemStatsViewModel> GetItemStats()
        {
            var statusData = await _context.Items
                .GroupBy(i => i.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var stats = new ItemStatsViewModel
            {
                TotalItems = await _context.Items.CountAsync(),
                BookCount = await _context.Books.CountAsync(),
                ToyCount = await _context.Toys.CountAsync(),
                MusicCount = await _context.Music.CountAsync(),
                OnOrderCount = await _context.Items.CountAsync(i => i.OnOrder),
                ReservedCount = await _context.Items.CountAsync(i => i.ReservedForBorrowerId != null),

                StatusCounts = statusData
                    .Select(x => new StatusCountViewModel
                    {
                        Status = x.Status.ToString(),
                        Count = x.Count
                    })
                    .ToList(),

                ItemsAddedPerMonth = (await _context.Items
                    .GroupBy(i => new { i.DateAdded.Year, i.DateAdded.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .Take(12)
                    .ToListAsync())
                    .Select(x => new MonthlyCountViewModel
                    {
                        Month = x.Year + "-" + x.Month.ToString("D2"),
                        Count = x.Count
                    })
                    .ToList(),

                PopularGenres = await _context.Books
                    .SelectMany(b => b.Genres)
                    .GroupBy(g => g.Name)
                    .Select(g => new GenreCountViewModel
                    {
                        Genre = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync()
            };

            return stats;
        }

        private async Task<FineStatsViewModel> GetFineStats()
        {
            var allFines = await _context.Loans
                .Where(l => l.Fine > 0)
                .Select(l => l.Fine ?? 0)
                .ToListAsync();

            var stats = new FineStatsViewModel
            {
                OutstandingFines = allFines.Any() ? allFines.Sum() : 0,
                TotalFinesCollected = 0,
                AverageFineAmount = allFines.Any() ? allFines.Average() : 0,

                BorrowersWithOutstandingFines = await _context.Loans
                    .Where(l => l.Fine > 0)
                    .Include(l => l.Borrower)
                    .GroupBy(l => l.BorrowerId)
                    .Select(g => new BorrowerFineViewModel
                    {
                        LibraryCard = g.First().Borrower.LibraryCard,
                        Name = g.First().Borrower.Name,
                        TotalFines = g.Sum(l => l.Fine ?? 0)
                    })
                    .OrderByDescending(x => x.TotalFines)
                    .ToListAsync()
            };

            return stats;
        }
    }
}