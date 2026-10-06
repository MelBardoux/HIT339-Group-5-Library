using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    /// Dashboard showing every simulated Email/SMS notification that has been sent.
    [Authorize(Roles = "Admin,Reception,Manager")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public NotificationsController(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        // GET: Notifications
        public async Task<IActionResult> Index(NotificationType? type, NotificationChannel? channel, string? search)
        {
            var query = _context.Notifications
                .Include(n => n.Borrower)
                .AsQueryable();

            if (type != null)
                query = query.Where(n => n.Type == type);

            if (channel != null)
                query = query.Where(n => n.Channel == channel);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(n => n.Borrower.Name.Contains(s)
                                      || n.Borrower.LibraryCard.Contains(s)
                                      || (n.ItemLibraryCode != null && n.ItemLibraryCode.Contains(s)));
            }

            var today = DateTime.Today;

            var viewModel = new NotificationIndexViewModel
            {
                Notifications = await query.OrderByDescending(n => n.SentAt).Take(200).ToListAsync(),
                TypeFilter = type,
                ChannelFilter = channel,
                Search = search,
                TotalCount = await _context.Notifications.CountAsync(),
                TodayCount = await _context.Notifications.CountAsync(n => n.SentAt >= today),
                EmailCount = await _context.Notifications.CountAsync(n => n.Channel == NotificationChannel.Email),
                SmsCount = await _context.Notifications.CountAsync(n => n.Channel == NotificationChannel.SMS)
            };

            return View(viewModel);
        }

        // POST: Notifications/RunCheck
        // Runs the due-soon / overdue check immediately (it also runs automatically every hour).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunCheck()
        {
            int sent = await _notifications.RunDueDateCheckAsync();
            TempData["SuccessMessage"] = sent == 0
                ? "Check complete - no new reminders were needed (each loan gets at most one per day)."
                : $"Check complete - {sent} reminder(s) sent on Email and SMS.";
            return RedirectToAction(nameof(Index));
        }
    }
}
