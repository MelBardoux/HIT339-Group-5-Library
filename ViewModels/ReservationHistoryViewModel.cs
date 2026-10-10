namespace LibrarySystem.ViewModels
{
    public class ReservationHistoryViewModel
    {
        public int Id { get; set; }
        public string ItemLibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string BorrowerLibraryCard { get; set; } = string.Empty;
        public DateTime PlacedAt { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}