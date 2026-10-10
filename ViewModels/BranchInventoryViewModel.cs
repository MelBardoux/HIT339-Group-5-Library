using LibrarySystem.Models;

namespace LibrarySystem.ViewModels
{
    public class BranchInventoryViewModel
    {
        public int? SelectedBranchId { get; set; }

        public string? SelectedType { get; set; }

        public List<Branch> Branches { get; set; } = new();

        public List<BranchInventoryItemViewModel> Items { get; set; } = new();
    }
}