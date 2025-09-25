using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Dtos;
using UniPortal.Services.Accounts;
using UniPortal.Services.Portals;

namespace UniPortal.Pages.Portals
{
    [Authorize]
    public class InboxModel : BasePageModel
    {
        private readonly InboxService _inboxService;

        public InboxModel(InboxService inboxService, AccountService accountService) : base(accountService)
        {
            _inboxService = inboxService;
        }

        public List<NotificationDto> Notifications { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public Guid? SelectedNotificationId { get; set; }

        public NotificationDto SelectedNotification { get; set; }

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalNotifications { get; set; }

        public async Task OnGetAsync()
        {
            await LoadNotificationsAsync();

            if (SelectedNotificationId.HasValue)
            {
                SelectedNotification = await _inboxService.GetNotificationByIdAsync(SelectedNotificationId.Value, CurrentAccount.Id);

                if (SelectedNotification != null && !SelectedNotification.IsRead)
                {
                    await _inboxService.MarkAsReadAsync(SelectedNotificationId.Value, CurrentAccount.Id);
                }
            }
        }

        private async Task LoadNotificationsAsync()
        {
            var role = CurrentRole;

            // 1. Get all notifications for the user (no paging yet)
            var allNotifications = await _inboxService.GetUserNotificationsAsync(CurrentAccount.Id, role);

            // 2. Apply filters
            var today = DateTime.Today;
            var filtered = allNotifications.AsQueryable();

            switch (Filter.ToLower())
            {
                case "unread":
                    filtered = filtered.Where(n => !n.IsRead);
                    break;
                case "today":
                    filtered = filtered.Where(n => n.CreatedAt.Date == today);
                    break;
                case "yesterday":
                    filtered = filtered.Where(n => n.CreatedAt.Date == today.AddDays(-1));
                    break;
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filtered = filtered.Where(n => n.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Sort newest first
            filtered = filtered.OrderByDescending(n => n.CreatedAt);

            // 4. Calculate total notifications and pages
            TotalNotifications = filtered.Count();
            TotalPages = (int)Math.Ceiling(TotalNotifications / (double)PageSize);

            // 5. Apply pagination
            Notifications = filtered
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
