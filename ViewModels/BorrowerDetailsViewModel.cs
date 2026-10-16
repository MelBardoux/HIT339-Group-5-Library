namespace LibrarySystem.ViewModels
{
    public class BorrowerDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCard { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}