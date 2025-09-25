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

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            await LoadNotificationsAsync();
        }

        /// <summary>
        /// Handler for marking a notification as read
        /// </summary>
        public async Task<IActionResult> OnGetReadAsync(Guid id)
        {
            // Mark the notification as read
            await _inboxService.MarkAsReadAsync(id, CurrentAccount.Id);

            // Get the notification to redirect to its file/path
            var notif = await _inboxService.GetNotificationByIdAsync(id, CurrentAccount.Id);
            return Redirect(notif?.FilePath ?? Url.Page("./Inbox"));
        }

        private async Task LoadNotificationsAsync()
        {
            var role = CurrentRole;

            var allNotifications = await _inboxService.GetUserNotificationsAsync(
                 CurrentAccount.Id, role, CurrentPage, PageSize);

            // Apply filters
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

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;

            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            Notifications = filteredList
                .OrderByDescending(n => n.CreatedAt)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
