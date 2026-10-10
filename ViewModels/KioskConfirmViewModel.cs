namespace LibrarySystem.ViewModels
{
    public class KioskConfirmViewModel
    {
        public string LibraryCard { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public bool IsReservation { get; set; }
    }
}