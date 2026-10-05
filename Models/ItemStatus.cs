namespace LibrarySystem.Models
{
    public enum ItemStatus
    {
        Available = 0,
        Reserved = 1,
        Borrowed = 2,
        Damaged = 3,
        Destroyed = 4,
        Lost = 5,
        InTransit = 6
    }
}