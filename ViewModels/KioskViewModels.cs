namespace LibrarySystem.ViewModels
{
    /// Basic borrower info shown at the top of every kiosk screen after scanning a card.
    public class KioskBorrowerViewModel
    {
        public string LibraryCard { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSuspended { get; set; }
        public decimal OutstandingFines { get; set; }
        public int ActiveLoanCount { get; set; }

        /// Suspended borrowers or borrowers with unpaid fines must see the front desk.
        public bool CanBorrow => !IsSuspended && OutstandingFines == 0;
    }

    public class KioskLoanRow
    {
        public string LibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public DateOnly BorrowedDate { get; set; }
        public DateOnly DueDate { get; set; }
        public decimal? Fine { get; set; }
        public DateOnly? ReturnedDate { get; set; }

        public int DaysLeft => DueDate.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber;
        public bool IsOverdue => ReturnedDate == null && DaysLeft < 0;
    }

    public class KioskCheckoutViewModel
    {
        public KioskBorrowerViewModel Borrower { get; set; } = new();

        /// Items this borrower checked out today (so they can see what they just scanned).
        public List<KioskLoanRow> CheckedOutToday { get; set; } = new();
    }

    public class KioskAccountViewModel
    {
        public KioskBorrowerViewModel Borrower { get; set; } = new();
        public List<KioskLoanRow> CurrentLoans { get; set; } = new();
        public List<KioskLoanRow> UnpaidFines { get; set; } = new();
        public List<string> ReservedItems { get; set; } = new();
    }
}
