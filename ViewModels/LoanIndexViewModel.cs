namespace LibrarySystem.ViewModels
{
    public class LoanIndexViewModel
    {
        public int Id { get; set; }
        public string ItemLibraryCode { get; set; }
        public string ItemName { get; set; }
        public string BorrowerName { get; set; }
        public string BorrowerLibraryCard { get; set; }
        public DateOnly BorrowedDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ReturnedDate { get; set; }
        public decimal? Fine { get; set; }
    }
}