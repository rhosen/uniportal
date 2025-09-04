using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Data.Entities;
using UniPortal.Services;

namespace UniPortal.Pages.Admin
{
    public class NotificationsModel : PageModel
    {
        private readonly NotificationService _notificationService;

        public NotificationsModel(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public List<Notification> Notifications { get; set; } = new();
        public List<NotificationType> NotificationTypes { get; set; } = new();

        [BindProperty] public Notification NewNotification { get; set; } = new();
        [BindProperty] public Notification EditNotification { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditNotificationId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            NotificationTypes = await _notificationService.GetNotificationTypesAsync();
            var allNotifications = await _notificationService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allNotifications = allNotifications
                    .Where(n => n.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allNotifications.Count / (double)PageSize);
            Notifications = allNotifications
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var userId = new Guid(User.Claims.First(c => c.Type == "sub").Value); // current user
            await _notificationService.CreateAsync(
                NewNotification.Title,
                NewNotification.Message,
                userId,
                NewNotification.NotificationTypeId,
                NewNotification.TargetId
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditNotificationId = id;
            var notif = await _notificationService.GetByIdAsync(id);
            if (notif != null)
            {
                EditNotification = new Notification
                {
                    Id = notif.Id,
                    Title = notif.Title,
                    Message = notif.Message,
                    NotificationTypeId = notif.NotificationTypeId,
                    TargetId = notif.TargetId
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditNotificationId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _notificationService.UpdateAsync(
                Guid.Parse(id),
                EditNotification.Title,
                EditNotification.Message,
                EditNotification.NotificationTypeId,
                EditNotification.TargetId
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _notificationService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _notificationService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
