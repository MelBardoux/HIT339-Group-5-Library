namespace LibrarySystem.ViewModels
{
    public class LoanReturnViewModel
    {
        public int Id { get; set; }
        public string ItemLibraryCode { get; set; }
        public string ItemName { get; set; }
        public string BorrowerName { get; set; }
        public string BorrowerLibraryCard { get; set; }
        public DateOnly BorrowedDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public decimal Fine { get; set; }
        public string? ReservedForName { get; set; }
    }
}