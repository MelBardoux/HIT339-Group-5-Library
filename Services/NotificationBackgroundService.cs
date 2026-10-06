namespace LibrarySystem.Services
{
    /// Runs the due-date / overdue check automatically when the app starts and then every hour,
    /// so reminders are sent without anyone clicking a button.
    public class NotificationBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(IServiceScopeFactory scopeFactory,
                                             ILogger<NotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Small delay so database seeding finishes first
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    int sent = await service.RunDueDateCheckAsync();
                    _logger.LogInformation("Automatic due-date check finished: {Count} reminder(s) sent.", sent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Automatic due-date check failed.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
