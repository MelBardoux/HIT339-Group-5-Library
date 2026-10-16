namespace LibrarySystem.ViewModels
{
    public class LoanIndexViewModel
    {
        public int Id { get; set; }
        public string ItemLibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string BorrowerLibraryCard { get; set; } = string.Empty;
        public DateOnly BorrowedDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ReturnedDate { get; set; }
        public decimal? Fine { get; set; }
    }
}