using Microsoft.AspNetCore.Mvc;
using UniPortal.Data.Entities;
using UniPortal.Services;

namespace UniPortal.Pages.Admin
{
    public class NotificationsModel : BasePageModel
    {
        private readonly NotificationService _notificationService;
        private readonly AccountService _accountService;

        public NotificationsModel(NotificationService notificationService, AccountService accountService) : base(accountService)
        {
            _notificationService = notificationService;
            _accountService = accountService;
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
            await _notificationService.CreateAsync(
                NewNotification.Title,
                NewNotification.Message,
                CurrentAccount.Id,
                NewNotification.NotificationTypeId,
                NewNotification.ReceiverId
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
                    ReceiverId = notif.ReceiverId,
                    UpdatedAt = DateTime.Now,
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
                EditNotification.ReceiverId
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
