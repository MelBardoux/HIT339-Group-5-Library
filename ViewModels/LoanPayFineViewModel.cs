namespace LibrarySystem.ViewModels
{
    public class LoanPayFineViewModel
    {
        public int Id { get; set; }
        public string ItemLibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal Fine { get; set; }
    }
}