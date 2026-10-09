using LibrarySystem.Models;

namespace LibrarySystem.ViewModels
{
    public class NotificationIndexViewModel
    {
        public List<Notification> Notifications { get; set; } = new();

        public NotificationType? TypeFilter { get; set; }
        public NotificationChannel? ChannelFilter { get; set; }
        public string? Search { get; set; }

        // Summary counts for the dashboard cards
        public int TotalCount { get; set; }
        public int TodayCount { get; set; }
        public int EmailCount { get; set; }
        public int SmsCount { get; set; }
    }
}
