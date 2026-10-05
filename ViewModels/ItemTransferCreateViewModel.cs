namespace LibrarySystem.ViewModels
{
    public class ItemTransferCreateViewModel
    {
        public int ItemId { get; set; }

        public string CurrentBranchName { get; set; } = string.Empty;

        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Items { get; set; } = new();
    }
}