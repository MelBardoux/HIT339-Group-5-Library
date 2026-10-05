using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Models
{
    public class StaffBranch
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
    }
}