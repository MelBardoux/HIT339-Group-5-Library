namespace LibrarySystem.ViewModels
{
    public class KioskReturnViewModel
    {
        public KioskBorrowerViewModel Borrower { get; set; } = new();
        public List<KioskReturnResult> ReturnedToday { get; set; } = new();
    }

    public class KioskReturnResult
    {
        public string LibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public decimal? Fine { get; set; }
        public bool WasOverdue { get; set; }
    }
}