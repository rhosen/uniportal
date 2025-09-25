using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Dtos;
using UniPortal.Helpers;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Operations
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class NotificationModel : BasePageModel
    {
        private readonly NotificationService _notificationService;

        public NotificationModel(NotificationService notificationService, AccountService accountService)
            : base(accountService)
        {
            _notificationService = notificationService;
        }

        // Notifications shown in UI (DTOs only)
        public List<NotificationDto> Notifications { get; set; } = new();

        // Recipient types for the select box (entity is fine for listing)
        public List<RecipientType> RecipientTypes { get; set; } = new();

        // Bind to DTOs for create/edit
        [BindProperty] public NotificationDto NewNotification { get; set; } = new();
        [BindProperty] public NotificationDto EditNotification { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditNotificationId { get; set; }

        // Files
        [BindProperty] public IFormFile? NewNotificationFile { get; set; }
        [BindProperty] public IFormFile? EditNotificationFile { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // load recipient types for the dropdown
            RecipientTypes = await _notificationService.GetNotificationTypesAsync();

            // get all notifications as DTOs
            var allNotifications = await _notificationService.GetAllAsync(); // returns List<NotificationDto>

            // apply search (in-memory; if dataset large, move filtering to service)
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();
                allNotifications = allNotifications
                    .Where(n => !string.IsNullOrEmpty(n.Title) &&
                                n.Title.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // paging
            TotalPages = (int)Math.Ceiling(allNotifications.Count / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;

            // clamp current page
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            Notifications = allNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // save file if uploaded
            if (NewNotificationFile != null)
            {
                NewNotification.FilePath = await FileHelper.SaveFileAsync(NewNotificationFile, UploadType.Notification);
            }

            await R(
                async () => await _notification_service_create_safe(),
                "Notification created successfully."
            );

            return RedirectToPage(new { CurrentPage, SearchTerm });

            // local function to keep the await expression tidy (so R receives Func<Task>)
            async Task _notification_service_create_safe()
            {
                // service will resolve RecipientNumber -> AccountId and validate
                await _notificationService.CreateAsync(NewNotification, CurrentAccount.Id);
            }
        }

        public async Task<IActionResult> OnPostSaveEditAsync()
        {
            if (EditNotificationFile != null)
            {
                EditNotification.FilePath = await FileHelper.SaveFileAsync(EditNotificationFile, UploadType.Notification);
            }

            await R(
                async () => await _notification_service_update_safe(),
                "Notification updated successfully."
            );

            return RedirectToPage(new { CurrentPage, SearchTerm });

            async Task _notification_service_update_safe()
            {
                // ensure EditNotification.Id is populated (form provides hidden id)
                await _notificationService.UpdateAsync(EditNotification);
            }
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            // set edit id so markup shows the edit row
            EditNotificationId = id;

            // get DTO from service
            var notifDto = await _notificationService.GetByIdAsync(id); // returns NotificationDto

            if (notifDto != null)
            {
                // populate the edit DTO (service returns RecipientNumber and FilePath already)
                EditNotification = notifDto;
            }
            else
            {
                // fallback empty
                EditNotification = new NotificationDto();
            }

            // reload list for the page
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditNotificationId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _notificationService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _notification_service_activate_safe();
            return RedirectToPage(new { CurrentPage, SearchTerm });

            async Task _notification_service_activate_safe()
            {
                await _notificationService.ActivateAsync(id);
            }
        }
    }
}
