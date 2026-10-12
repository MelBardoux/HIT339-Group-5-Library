using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class LoanBorrowViewModel
    {
        [Required]
        public string LibraryCard { get; set; }

        public List<string> LibraryCodes { get; set; } = new();

        public BorrowerLookupResult? BorrowerResult { get; set; }
        public List<ItemLookupResult> ItemResults { get; set; } = new();

        public bool IsLookedUp { get; set; }
        public bool IsConfirmed { get; set; }
        public bool BorrowerOverride { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class BorrowerLookupResult
    {
        public int Id { get; set; }
        public string LibraryCard { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public bool IsSuspended { get; set; }
        public decimal OutstandingFines { get; set; }
    }

    public class ItemLookupResult
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool IsAvailable { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }
}