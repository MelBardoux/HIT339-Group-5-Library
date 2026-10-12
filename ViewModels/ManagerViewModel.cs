namespace LibrarySystem.ViewModels
{
    public class ManagerViewModel
    {
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<BranchFilterViewModel> Branches { get; set; } = new();

        public BorrowingStatsViewModel BorrowingStats { get; set; }
        public ItemStatsViewModel ItemStats { get; set; }
        public FineStatsViewModel FineStats { get; set; }
        public ReservationStatsViewModel ReservationStats { get; set; }
    }

    public class BorrowingStatsViewModel
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public double AverageLoanDurationDays { get; set; }
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
        public int DamagedCount { get; set; }
        public int LostCount { get; set; }
        public int DestroyedCount { get; set; }
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
        public List<MonthlyDecimalViewModel> FinesPerMonth { get; set; } = new();
        public List<BranchFineViewModel> FinesPerBranch { get; set; } = new();
        public List<BorrowerFineViewModel> BorrowersWithOutstandingFines { get; set; } = new();
    }

    public class ReservationStatsViewModel
    {
        public int TotalReservations { get; set; }
        public int WaitingCount { get; set; }
        public int ReadyCount { get; set; }
        public int CollectedCount { get; set; }
        public int CancelledCount { get; set; }
        public int ExpiredCount { get; set; }
        public double AverageWaitDays { get; set; }
        public List<ReservationByBranchViewModel> ReservationsByBranch { get; set; } = new();
        public List<MostReservedItemViewModel> MostReservedItems { get; set; } = new();
    }

    public class ReservationByBranchViewModel
    {
        public string BranchName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MostReservedItemViewModel
    {
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int ReservationCount { get; set; }
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

    public class BranchFilterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class MonthlyDecimalViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class BranchFineViewModel
    {
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalFines { get; set; }
        public int FineCount { get; set; }
    }
}