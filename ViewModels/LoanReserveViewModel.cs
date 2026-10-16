namespace LibrarySystem.ViewModels
{
    /// ViewModel for the reservation confirmation page.
    /// Shows item and borrower details, plus the borrower's position in the waitlist queue.
    public class LoanReserveViewModel
    {
        public string ItemLibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemStatus { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string BorrowerLibraryCard { get; set; } = string.Empty;

        // The borrower's position in the waitlist queue (e.g. 1 = next in line)
        public int QueuePosition { get; set; }

        // How many reservations already exist for this item
        public int ExistingReservations { get; set; }
    }
}