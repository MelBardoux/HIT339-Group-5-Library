using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        // GET: Manager Dashboard with optional branch and date filters
        public async Task<IActionResult> Index(int? branchId, DateTime? startDate, DateTime? endDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Load branch list for the filter dropdown
            var branches = await _context.Branches
                .OrderBy(b => b.Name)
                .Select(b => new BranchFilterViewModel { Id = b.Id, Name = b.Name })
                .ToListAsync();

            // Find selected branch name for display
            var branchName = branchId.HasValue
                ? branches.FirstOrDefault(b => b.Id == branchId.Value)?.Name
                : null;

            var viewModel = new ManagerViewModel
            {
                BranchId = branchId,
                BranchName = branchName,
                StartDate = startDate,
                EndDate = endDate,
                Branches = branches,
                BorrowingStats = await GetBorrowingStats(today, branchId, startDate, endDate),
                ItemStats = await GetItemStats(branchId),
                FineStats = await GetFineStats(branchId, startDate, endDate),
                ReservationStats = await GetReservationStats(branchId)
            };

            return View(viewModel);
        }

        private async Task<BorrowingStatsViewModel> GetBorrowingStats(
            DateOnly today, int? branchId, DateTime? startDate, DateTime? endDate)
        {
            // Base query with optional branch and date range filters
            var loansQuery = _context.Loans.Include(l => l.Item).AsQueryable();

            if (branchId.HasValue)
                loansQuery = loansQuery.Where(l => l.Item.BranchId == branchId.Value);

            if (startDate.HasValue)
            {
                var start = DateOnly.FromDateTime(startDate.Value);
                loansQuery = loansQuery.Where(l => l.BorrowedDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateOnly.FromDateTime(endDate.Value);
                loansQuery = loansQuery.Where(l => l.BorrowedDate <= end);
            }

            // Calculate average loan duration from returned loans
            var returnedLoans = await loansQuery
                .Where(l => l.ReturnedDate != null)
                .Select(l => new { l.BorrowedDate, l.ReturnedDate, l.DueDate })
                .ToListAsync();

            var avgDuration = returnedLoans.Any()
                ? returnedLoans.Average(l =>
                    (l.ReturnedDate!.Value.ToDateTime(TimeOnly.MinValue) -
                     l.BorrowedDate.ToDateTime(TimeOnly.MinValue)).TotalDays)
                : 0;

            // Return rate: percentage of returned loans that were on time
            var returnRate = returnedLoans.Any()
                ? (double)returnedLoans.Count(l => l.ReturnedDate!.Value <= l.DueDate)
                  / returnedLoans.Count * 100
                : 0;

            var stats = new BorrowingStatsViewModel
            {
                TotalLoans = await loansQuery.CountAsync(),
                ActiveLoans = await loansQuery.CountAsync(l => l.ReturnedDate == null),
                OverdueLoans = await loansQuery.CountAsync(l => l.ReturnedDate == null && l.DueDate < today),
                AverageLoanDurationDays = Math.Round(avgDuration, 1),
                ReturnRate = Math.Round(returnRate, 1),

                MostBorrowedItems = await loansQuery
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

                BusiestBorrowers = await loansQuery
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

                LoansPerMonth = (await loansQuery
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

        private async Task<ItemStatsViewModel> GetItemStats(int? branchId)
        {
            // Base query with optional branch filter (no date filter for inventory)
            var itemsQuery = _context.Items.AsQueryable();

            if (branchId.HasValue)
                itemsQuery = itemsQuery.Where(i => i.BranchId == branchId.Value);

            var statusData = await itemsQuery
                .GroupBy(i => i.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Calculate turnover: average borrow count per item
            var totalItems = await itemsQuery.CountAsync();
            var totalLoansForItems = branchId.HasValue
                ? await _context.Loans.CountAsync(l => l.Item.BranchId == branchId.Value)
                : await _context.Loans.CountAsync();
            var turnover = totalItems > 0 ? (double)totalLoansForItems / totalItems : 0;

            var stats = new ItemStatsViewModel
            {
                TotalItems = totalItems,
                BookCount = await itemsQuery.OfType<Book>().CountAsync(),
                ToyCount = await itemsQuery.OfType<Toy>().CountAsync(),
                MusicCount = await itemsQuery.OfType<Music>().CountAsync(),
                OnOrderCount = await itemsQuery.CountAsync(i => i.OnOrder),
                ReservedCount = await itemsQuery.CountAsync(i => i.ReservedForBorrowerId != null),
                DamagedCount = await itemsQuery.CountAsync(i => i.Status == ItemStatus.Damaged),
                LostCount = await itemsQuery.CountAsync(i => i.Status == ItemStatus.Lost),
                DestroyedCount = await itemsQuery.CountAsync(i => i.Status == ItemStatus.Destroyed),
                AverageTurnoverRate = Math.Round(turnover, 2),

                StatusCounts = statusData
                    .Select(x => new StatusCountViewModel
                    {
                        Status = x.Status.ToString(),
                        Count = x.Count
                    })
                    .ToList(),

                ItemsAddedPerMonth = (await itemsQuery
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

                PopularGenres = branchId.HasValue
                    ? await _context.Books
                        .Where(b => b.BranchId == branchId.Value)
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
                    : await _context.Books
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

        private async Task<FineStatsViewModel> GetFineStats(
            int? branchId, DateTime? startDate, DateTime? endDate)
        {
            // Base query with optional branch and date filters
            var finesQuery = _context.Loans
                .Include(l => l.Item)
                .Include(l => l.Borrower)
                .Where(l => l.Fine > 0);

            if (branchId.HasValue)
                finesQuery = finesQuery.Where(l => l.Item.BranchId == branchId.Value);

            if (startDate.HasValue)
            {
                var start = DateOnly.FromDateTime(startDate.Value);
                finesQuery = finesQuery.Where(l => l.BorrowedDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateOnly.FromDateTime(endDate.Value);
                finesQuery = finesQuery.Where(l => l.BorrowedDate <= end);
            }

            var allFines = await finesQuery
                .Select(l => l.Fine ?? 0)
                .ToListAsync();

            // Monthly fine totals for the trend table
            var finesPerMonth = (await finesQuery
                .GroupBy(l => new { l.BorrowedDate.Year, l.BorrowedDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(l => l.Fine ?? 0)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12)
                .ToListAsync())
                .Select(x => new MonthlyDecimalViewModel
                {
                    Month = x.Year + "-" + x.Month.ToString("D2"),
                    Amount = x.Total
                })
                .ToList();

            // Fine totals grouped by branch
            var finesPerBranch = await finesQuery
                .GroupBy(l => l.Item.Branch.Name)
                .Select(g => new BranchFineViewModel
                {
                    BranchName = g.Key,
                    TotalFines = g.Sum(l => l.Fine ?? 0),
                    FineCount = g.Count()
                })
                .OrderByDescending(x => x.TotalFines)
                .ToListAsync();

            var stats = new FineStatsViewModel
            {
                OutstandingFines = allFines.Any() ? allFines.Sum() : 0,
                TotalFinesCollected = 0,
                AverageFineAmount = allFines.Any() ? allFines.Average() : 0,
                FinesPerMonth = finesPerMonth,
                FinesPerBranch = finesPerBranch,

                BorrowersWithOutstandingFines = await finesQuery
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

        private async Task<ReservationStatsViewModel> GetReservationStats(int? branchId)
        {
            var query = _context.Reservations
                .Include(r => r.Item)
                .Include(r => r.Branch)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(r => r.BranchId == branchId.Value);

            var all = await query.ToListAsync();

            // Calculate average wait time from collected reservations
            var collected = all.Where(r => r.Status == Models.ReservationStatus.Collected && r.NotifiedAt.HasValue).ToList();
            var avgWait = collected.Any()
                ? collected.Average(r => (r.NotifiedAt!.Value - r.PlacedAt).TotalDays)
                : 0;

            return new ReservationStatsViewModel
            {
                TotalReservations = all.Count,
                WaitingCount = all.Count(r => r.Status == Models.ReservationStatus.Waiting),
                ReadyCount = all.Count(r => r.Status == Models.ReservationStatus.Ready),
                CollectedCount = all.Count(r => r.Status == Models.ReservationStatus.Collected),
                CancelledCount = all.Count(r => r.Status == Models.ReservationStatus.Cancelled),
                ExpiredCount = all.Count(r => r.Status == Models.ReservationStatus.Expired),
                AverageWaitDays = Math.Round(avgWait, 1),

                ReservationsByBranch = all
                    .GroupBy(r => r.Branch.Name)
                    .Select(g => new ReservationByBranchViewModel
                    {
                        BranchName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),

                MostReservedItems = all
                    .GroupBy(r => r.ItemId)
                    .Select(g => new MostReservedItemViewModel
                    {
                        LibraryCode = g.First().Item.LibraryCode,
                        Name = g.First().Item.Name,
                        ReservationCount = g.Count()
                    })
                    .OrderByDescending(x => x.ReservationCount)
                    .Take(5)
                    .ToList()
            };
        }

        // CSV export for the Borrowing statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportBorrowingCsv(int? branchId, DateTime? startDate, DateTime? endDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var stats = await GetBorrowingStats(today, branchId, startDate, endDate);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Report,Borrowing Statistics");
            csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm}");
            if (branchId.HasValue)
                csv.AppendLine($"Branch,{(await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"}");
            if (startDate.HasValue)
                csv.AppendLine($"Start Date,{startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                csv.AppendLine($"End Date,{endDate.Value:yyyy-MM-dd}");
            csv.AppendLine();

            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Loans,{stats.TotalLoans}");
            csv.AppendLine($"Active Loans,{stats.ActiveLoans}");
            csv.AppendLine($"Overdue Loans,{stats.OverdueLoans}");
            csv.AppendLine($"Avg Loan Duration (Days),{stats.AverageLoanDurationDays}");
            csv.AppendLine($"On-Time Return Rate (%),{stats.ReturnRate}");
            csv.AppendLine();

            csv.AppendLine("Most Borrowed Items");
            csv.AppendLine("Library Code,Name,Type,Times Borrowed");
            foreach (var item in stats.MostBorrowedItems)
                csv.AppendLine($"{item.LibraryCode},{item.Name},{item.Type},{item.BorrowCount}");
            csv.AppendLine();

            csv.AppendLine("Busiest Borrowers");
            csv.AppendLine("Library Card,Name,Loans");
            foreach (var b in stats.BusiestBorrowers)
                csv.AppendLine($"{b.LibraryCard},{b.Name},{b.LoanCount}");
            csv.AppendLine();

            csv.AppendLine("Loans Per Month");
            csv.AppendLine("Month,Loans");
            foreach (var m in stats.LoansPerMonth)
                csv.AppendLine($"{m.Month},{m.Count}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"BorrowingReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // CSV export for the Items statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportItemsCsv(int? branchId)
        {
            var stats = await GetItemStats(branchId);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Report,Inventory Health");
            csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm}");
            if (branchId.HasValue)
                csv.AppendLine($"Branch,{(await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"}");
            csv.AppendLine();

            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Items,{stats.TotalItems}");
            csv.AppendLine($"Books,{stats.BookCount}");
            csv.AppendLine($"Toys,{stats.ToyCount}");
            csv.AppendLine($"Music,{stats.MusicCount}");
            csv.AppendLine($"On Order,{stats.OnOrderCount}");
            csv.AppendLine($"Reserved,{stats.ReservedCount}");
            csv.AppendLine($"Damaged,{stats.DamagedCount}");
            csv.AppendLine($"Lost,{stats.LostCount}");
            csv.AppendLine($"Destroyed,{stats.DestroyedCount}");
            csv.AppendLine($"Avg Turnover Rate,{stats.AverageTurnoverRate}");
            csv.AppendLine();

            csv.AppendLine("Items by Status");
            csv.AppendLine("Status,Count");
            foreach (var s in stats.StatusCounts)
                csv.AppendLine($"{s.Status},{s.Count}");
            csv.AppendLine();

            csv.AppendLine("Popular Book Genres");
            csv.AppendLine("Genre,Count");
            foreach (var g in stats.PopularGenres)
                csv.AppendLine($"{g.Genre},{g.Count}");
            csv.AppendLine();

            csv.AppendLine("Items Added Per Month");
            csv.AppendLine("Month,Count");
            foreach (var m in stats.ItemsAddedPerMonth)
                csv.AppendLine($"{m.Month},{m.Count}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"InventoryReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // CSV export for the Fines statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportFinesCsv(int? branchId, DateTime? startDate, DateTime? endDate)
        {
            var stats = await GetFineStats(branchId, startDate, endDate);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Report,Fine Revenue Audit");
            csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm}");
            if (branchId.HasValue)
                csv.AppendLine($"Branch,{(await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"}");
            if (startDate.HasValue)
                csv.AppendLine($"Start Date,{startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                csv.AppendLine($"End Date,{endDate.Value:yyyy-MM-dd}");
            csv.AppendLine();

            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Outstanding Fines,{stats.OutstandingFines:F2}");
            csv.AppendLine($"Total Collected,{stats.TotalFinesCollected:F2}");
            csv.AppendLine($"Average Fine,{stats.AverageFineAmount:F2}");
            csv.AppendLine();

            csv.AppendLine("Fines by Branch");
            csv.AppendLine("Branch,Fine Count,Total");
            foreach (var b in stats.FinesPerBranch)
                csv.AppendLine($"{b.BranchName},{b.FineCount},{b.TotalFines:F2}");
            csv.AppendLine();

            csv.AppendLine("Fines by Month");
            csv.AppendLine("Month,Total");
            foreach (var m in stats.FinesPerMonth)
                csv.AppendLine($"{m.Month},{m.Amount:F2}");
            csv.AppendLine();

            csv.AppendLine("Borrowers with Outstanding Fines");
            csv.AppendLine("Library Card,Name,Total Fines");
            foreach (var b in stats.BorrowersWithOutstandingFines)
                csv.AppendLine($"{b.LibraryCard},{b.Name},{b.TotalFines:F2}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"FineReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // CSV export for the Reservations statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportReservationsCsv(int? branchId)
        {
            var stats = await GetReservationStats(branchId);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Report,Reservation Statistics");
            csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm}");
            if (branchId.HasValue)
                csv.AppendLine($"Branch,{(await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"}");
            csv.AppendLine();

            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Reservations,{stats.TotalReservations}");
            csv.AppendLine($"Waiting,{stats.WaitingCount}");
            csv.AppendLine($"Ready,{stats.ReadyCount}");
            csv.AppendLine($"Collected,{stats.CollectedCount}");
            csv.AppendLine($"Cancelled,{stats.CancelledCount}");
            csv.AppendLine($"Expired,{stats.ExpiredCount}");
            csv.AppendLine($"Avg Wait (Days),{stats.AverageWaitDays}");
            csv.AppendLine();

            csv.AppendLine("Reservations by Branch");
            csv.AppendLine("Branch,Count");
            foreach (var b in stats.ReservationsByBranch)
                csv.AppendLine($"{b.BranchName},{b.Count}");
            csv.AppendLine();

            csv.AppendLine("Most Reserved Items");
            csv.AppendLine("Library Code,Name,Reservations");
            foreach (var item in stats.MostReservedItems)
                csv.AppendLine($"{item.LibraryCode},{item.Name},{item.ReservationCount}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"ReservationReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // PDF export for the Borrowing statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportBorrowingPdf(int? branchId, DateTime? startDate, DateTime? endDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var stats = await GetBorrowingStats(today, branchId, startDate, endDate);

            // Resolve branch name for the header if filtered
            var branchNameText = branchId.HasValue
                ? (await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"
                : null;

            // Build the PDF document using QuestPDF fluent API
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Page header with report title and filter info
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Borrowing Statistics Report")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (branchNameText != null)
                            col.Item().Text($"Branch: {branchNameText}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (startDate.HasValue)
                            col.Item().Text($"Start Date: {startDate.Value:yyyy-MM-dd}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (endDate.HasValue)
                            col.Item().Text($"End Date: {endDate.Value:yyyy-MM-dd}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    // Page content with metrics and tables
                    page.Content().Column(col =>
                    {
                        // Key metrics section
                        col.Item().Text("Key Metrics").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Total Loans: {stats.TotalLoans}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Active Loans: {stats.ActiveLoans}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Overdue Loans: {stats.OverdueLoans}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Avg Duration: {stats.AverageLoanDurationDays} days");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"On-Time Return Rate: {stats.ReturnRate}%");
                        });

                        col.Item().PaddingTop(10);

                        // Most borrowed items table
                        col.Item().Text("Most Borrowed Items").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });
                            // Table header row
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Library Code").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Name").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Type").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Borrows").Bold();
                            // Table data rows
                            foreach (var item in stats.MostBorrowedItems)
                            {
                                table.Cell().Border(1).Padding(4).Text(item.LibraryCode);
                                table.Cell().Border(1).Padding(4).Text(item.Name);
                                table.Cell().Border(1).Padding(4).Text(item.Type);
                                table.Cell().Border(1).Padding(4).Text(item.BorrowCount.ToString());
                            }
                        });

                        col.Item().PaddingTop(10);

                        // Busiest borrowers table
                        col.Item().Text("Busiest Borrowers").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Library Card").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Name").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Loans").Bold();
                            foreach (var b in stats.BusiestBorrowers)
                            {
                                table.Cell().Border(1).Padding(4).Text(b.LibraryCard);
                                table.Cell().Border(1).Padding(4).Text(b.Name);
                                table.Cell().Border(1).Padding(4).Text(b.LoanCount.ToString());
                            }
                        });

                        col.Item().PaddingTop(10);

                        // Loans per month table
                        col.Item().Text("Loans Per Month").FontSize(13).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Month").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Loans").Bold();
                            foreach (var m in stats.LoansPerMonth)
                            {
                                table.Cell().Border(1).Padding(4).Text(m.Month);
                                table.Cell().Border(1).Padding(4).Text(m.Count.ToString());
                            }
                        });
                    });

                    // Page footer with page number
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            // Generate PDF bytes and return as file download
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"BorrowingReport_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // PDF export for the Items statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportItemsPdf(int? branchId)
        {
            var stats = await GetItemStats(branchId);

            // Resolve branch name for the header if filtered
            var branchNameText = branchId.HasValue
                ? (await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"
                : null;

            // Build the PDF document using QuestPDF fluent API
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Page header with report title and filter info
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Inventory Health Report")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (branchNameText != null)
                            col.Item().Text($"Branch: {branchNameText}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    // Page content with metrics and tables
                    page.Content().Column(col =>
                    {
                        // Key metrics section
                        col.Item().Text("Key Metrics").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Total Items: {stats.TotalItems}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Books: {stats.BookCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Toys: {stats.ToyCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Music: {stats.MusicCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"On Order: {stats.OnOrderCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Reserved: {stats.ReservedCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Damaged: {stats.DamagedCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Lost: {stats.LostCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Destroyed: {stats.DestroyedCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Avg Turnover: {stats.AverageTurnoverRate}");
                        });

                        col.Item().PaddingTop(10);

                        // Items by status table
                        col.Item().Text("Items by Status").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Status").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Count").Bold();
                            foreach (var s in stats.StatusCounts)
                            {
                                table.Cell().Border(1).Padding(4).Text(s.Status);
                                table.Cell().Border(1).Padding(4).Text(s.Count.ToString());
                            }
                        });

                        col.Item().PaddingTop(10);

                        // Popular genres table
                        col.Item().Text("Popular Book Genres").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Genre").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Count").Bold();
                            foreach (var g in stats.PopularGenres)
                            {
                                table.Cell().Border(1).Padding(4).Text(g.Genre);
                                table.Cell().Border(1).Padding(4).Text(g.Count.ToString());
                            }
                        });

                        col.Item().PaddingTop(10);

                        // Items added per month table
                        col.Item().Text("Items Added Per Month").FontSize(13).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Month").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Count").Bold();
                            foreach (var m in stats.ItemsAddedPerMonth)
                            {
                                table.Cell().Border(1).Padding(4).Text(m.Month);
                                table.Cell().Border(1).Padding(4).Text(m.Count.ToString());
                            }
                        });
                    });

                    // Page footer with page number
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            // Generate PDF bytes and return as file download
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"InventoryReport_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // PDF export for the Fines statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportFinesPdf(int? branchId, DateTime? startDate, DateTime? endDate)
        {
            var stats = await GetFineStats(branchId, startDate, endDate);

            // Resolve branch name for the header if filtered
            var branchNameText = branchId.HasValue
                ? (await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"
                : null;

            // Build the PDF document using QuestPDF fluent API
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Page header with report title and filter info
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Fine Revenue Audit Report")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (branchNameText != null)
                            col.Item().Text($"Branch: {branchNameText}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (startDate.HasValue)
                            col.Item().Text($"Start Date: {startDate.Value:yyyy-MM-dd}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (endDate.HasValue)
                            col.Item().Text($"End Date: {endDate.Value:yyyy-MM-dd}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    // Page content with metrics and tables
                    page.Content().Column(col =>
                    {
                        // Key metrics section
                        col.Item().Text("Key Metrics").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Outstanding Fines: ${stats.OutstandingFines:F2}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Total Collected: ${stats.TotalFinesCollected:F2}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Average Fine: ${stats.AverageFineAmount:F2}");
                        });

                        col.Item().PaddingTop(10);

                        // Fines by branch table
                        col.Item().Text("Fines by Branch").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Branch").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Fine Count").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Total").Bold();
                            foreach (var b in stats.FinesPerBranch)
                            {
                                table.Cell().Border(1).Padding(4).Text(b.BranchName);
                                table.Cell().Border(1).Padding(4).Text(b.FineCount.ToString());
                                table.Cell().Border(1).Padding(4).Text($"${b.TotalFines:F2}");
                            }
                        });

                        col.Item().PaddingTop(10);

                        // Fines by month table
                        col.Item().Text("Fines by Month").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Month").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Total").Bold();
                            foreach (var m in stats.FinesPerMonth)
                            {
                                table.Cell().Border(1).Padding(4).Text(m.Month);
                                table.Cell().Border(1).Padding(4).Text($"${m.Amount:F2}");
                            }
                        });

                        col.Item().PaddingTop(10);

                        // Borrowers with outstanding fines table
                        col.Item().Text("Borrowers with Outstanding Fines").FontSize(13).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Library Card").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Name").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Total Fines").Bold();
                            foreach (var b in stats.BorrowersWithOutstandingFines)
                            {
                                table.Cell().Border(1).Padding(4).Text(b.LibraryCard);
                                table.Cell().Border(1).Padding(4).Text(b.Name);
                                table.Cell().Border(1).Padding(4).Text($"${b.TotalFines:F2}");
                            }
                        });
                    });

                    // Page footer with page number
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            // Generate PDF bytes and return as file download
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"FineReport_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // PDF export for the Reservations statistics tab
        [HttpGet]
        public async Task<IActionResult> ExportReservationsPdf(int? branchId)
        {
            var stats = await GetReservationStats(branchId);

            var branchNameText = branchId.HasValue
                ? (await _context.Branches.FindAsync(branchId.Value))?.Name ?? "Unknown"
                : null;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Reservation Statistics Report")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (branchNameText != null)
                            col.Item().Text($"Branch: {branchNameText}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Key Metrics").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Total Reservations: {stats.TotalReservations}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Waiting: {stats.WaitingCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Ready: {stats.ReadyCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Collected: {stats.CollectedCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Cancelled: {stats.CancelledCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Expired: {stats.ExpiredCount}");
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                .Text($"Avg Wait: {stats.AverageWaitDays} days");
                        });

                        col.Item().PaddingTop(10);

                        col.Item().Text("Reservations by Branch").FontSize(13).Bold();
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Branch").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Count").Bold();
                            foreach (var b in stats.ReservationsByBranch)
                            {
                                table.Cell().Border(1).Padding(4).Text(b.BranchName);
                                table.Cell().Border(1).Padding(4).Text(b.Count.ToString());
                            }
                        });

                        col.Item().PaddingTop(10);

                        col.Item().Text("Most Reserved Items").FontSize(13).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                            });
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Library Code").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Name").Bold();
                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                                .Text("Reservations").Bold();
                            foreach (var item in stats.MostReservedItems)
                            {
                                table.Cell().Border(1).Padding(4).Text(item.LibraryCode);
                                table.Cell().Border(1).Padding(4).Text(item.Name);
                                table.Cell().Border(1).Padding(4).Text(item.ReservationCount.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"ReservationReport_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}