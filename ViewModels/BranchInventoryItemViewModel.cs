namespace LibrarySystem.ViewModels
{
    public class BranchInventoryItemViewModel
    {
        public int Id { get; set; }

        public string LibraryCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string BranchName { get; set; } = string.Empty;

        public bool OnOrder { get; set; }
    }
}