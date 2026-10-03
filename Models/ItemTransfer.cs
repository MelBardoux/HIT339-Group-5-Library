using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class ItemTransfer
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int FromBranchId { get; set; }
        public Branch FromBranch { get; set; } = null!;

        public int ToBranchId { get; set; }
        public Branch ToBranch { get; set; } = null!;

        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; } // nullable since a transfer request thats pending wont have a completion date
    }
}