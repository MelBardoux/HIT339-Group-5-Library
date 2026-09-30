namespace LibrarySystem.ViewModels
{
    public class BorrowerDetailsViewModel
    {
        public int Id { get; set; }
        public string LibraryCard { get; set; }
        public string Name { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
}