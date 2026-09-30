namespace LibrarySystem.ViewModels
{
    public class ToyDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AgeDisplay { get; set; }
        public bool BatteryRequired { get; set; }
        public List<string> Types { get; set; } = new();
        public string Status { get; set; }
        public DateOnly DateAdded { get; set; }
    }
}