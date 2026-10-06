namespace LibrarySystem.Models
{
    /// The event that caused a notification to be sent.
    public enum NotificationType
    {
        ItemBorrowed = 0,
        DueSoon = 1,
        Overdue = 2,
        FineCharged = 3,
        ItemAvailable = 4
    }
}
