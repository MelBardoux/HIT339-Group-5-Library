namespace LibrarySystem.ViewModels
{
    public class ItemTransferIndexViewModel
    {
        public int Id { get; set; }

        public string ItemLibraryCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;

        public string FromBranchName { get; set; } = string.Empty;
        public string ToBranchName { get; set; } = string.Empty;

        public string RequestedByUserEmail { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanReceive { get; set; }
    }
}