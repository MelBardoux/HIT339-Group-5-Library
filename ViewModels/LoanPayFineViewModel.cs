namespace LibrarySystem.ViewModels
{
    public class LoanPayFineViewModel
    {
        public int Id { get; set; }
        public string ItemLibraryCode { get; set; }
        public string ItemName { get; set; }
        public string BorrowerName { get; set; }
        public decimal Fine { get; set; }
    }
}