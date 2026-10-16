namespace LibrarySystem.ViewModels
{
    public class ToyIndexViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AgeDisplay { get; set; } = string.Empty;
        public bool BatteryRequired { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool OnOrder { get; set; }
        public string? ReservedForCard { get; set; }
    }
}