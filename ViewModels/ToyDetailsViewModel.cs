namespace LibrarySystem.ViewModels
{
    public class ToyDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AgeDisplay { get; set; } = string.Empty;
        public bool BatteryRequired { get; set; }
        public List<string> Types { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateOnly DateAdded { get; set; }
    }
}