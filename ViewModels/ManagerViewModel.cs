namespace LibrarySystem.ViewModels
{
    public class ManagerViewModel
    {
        // Filter properties bound from the query string
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Dropdown options for the branch filter
        public List<BranchFilterViewModel> Branches { get; set; } = new();

        public BorrowingStatsViewModel BorrowingStats { get; set; }
        public ItemStatsViewModel ItemStats { get; set; }
        public FineStatsViewModel FineStats { get; set; }
    }

    public class BorrowingStatsViewModel
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        // Average number of days items are kept before return
        public double AverageLoanDurationDays { get; set; }
        // Percentage of loans returned on time
        public double ReturnRate { get; set; }
        public List<MostBorrowedItemViewModel> MostBorrowedItems { get; set; } = new();
        public List<BusiestBorrowerViewModel> BusiestBorrowers { get; set; } = new();
        public List<MonthlyCountViewModel> LoansPerMonth { get; set; } = new();
    }

    public class ItemStatsViewModel
    {
        public int TotalItems { get; set; }
        public int BookCount { get; set; }
        public int ToyCount { get; set; }
        public int MusicCount { get; set; }
        public int OnOrderCount { get; set; }
        public int ReservedCount { get; set; }
        // Number of items marked Damaged, Lost, or Destroyed
        public int DamagedCount { get; set; }
        public int LostCount { get; set; }
        public int DestroyedCount { get; set; }
        // Average times each item has been borrowed
        public double AverageTurnoverRate { get; set; }
        public List<StatusCountViewModel> StatusCounts { get; set; } = new();
        public List<MonthlyCountViewModel> ItemsAddedPerMonth { get; set; } = new();
        public List<GenreCountViewModel> PopularGenres { get; set; } = new();
    }

    public class FineStatsViewModel
    {
        public decimal TotalFinesCollected { get; set; }
        public decimal OutstandingFines { get; set; }
        public decimal AverageFineAmount { get; set; }
        // Fine totals grouped by month for trend display
        public List<MonthlyDecimalViewModel> FinesPerMonth { get; set; } = new();
        // Fine totals grouped by branch
        public List<BranchFineViewModel> FinesPerBranch { get; set; } = new();
        public List<BorrowerFineViewModel> BorrowersWithOutstandingFines { get; set; } = new();
    }

    public class MostBorrowedItemViewModel
    {
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int BorrowCount { get; set; }
    }

    public class BusiestBorrowerViewModel
    {
        public string LibraryCard { get; set; }
        public string Name { get; set; }
        public int LoanCount { get; set; }
    }

    public class MonthlyCountViewModel
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }

    public class StatusCountViewModel
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class GenreCountViewModel
    {
        public string Genre { get; set; }
        public int Count { get; set; }
    }

    public class BorrowerFineViewModel
    {
        public string LibraryCard { get; set; }
        public string Name { get; set; }
        public decimal TotalFines { get; set; }
    }

    // Used to populate the branch filter dropdown on the dashboard
    public class BranchFilterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Monthly fine totals for the fine revenue trend table
    public class MonthlyDecimalViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    // Fine totals broken down by branch
    public class BranchFineViewModel
    {
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalFines { get; set; }
        public int FineCount { get; set; }
    }
}