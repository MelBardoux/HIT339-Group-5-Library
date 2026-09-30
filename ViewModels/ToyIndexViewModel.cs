namespace LibrarySystem.ViewModels
{
    public class ToyIndexViewModel
    {
        public int Id { get; set; }
        public string LibraryCode { get; set; }
        public string Name { get; set; }
        public string AgeDisplay { get; set; }
        public bool BatteryRequired { get; set; }
        public string Status { get; set; }
        public bool OnOrder { get; set; }
        public string? ReservedForCard { get; set; }
    }
}