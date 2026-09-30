namespace LibrarySystem.ViewModels
{
    public class ManagerViewModel
    {
        public BorrowingStatsViewModel BorrowingStats { get; set; }
        public ItemStatsViewModel ItemStats { get; set; }
        public FineStatsViewModel FineStats { get; set; }
    }

    public class BorrowingStatsViewModel
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
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
        public List<StatusCountViewModel> StatusCounts { get; set; } = new();
        public List<MonthlyCountViewModel> ItemsAddedPerMonth { get; set; } = new();
        public List<GenreCountViewModel> PopularGenres { get; set; } = new();
    }

    public class FineStatsViewModel
    {
        public decimal TotalFinesCollected { get; set; }
        public decimal OutstandingFines { get; set; }
        public decimal AverageFineAmount { get; set; }
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
}