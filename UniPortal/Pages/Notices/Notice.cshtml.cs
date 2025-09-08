using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Accounts;
using UniPortal.Services.Notices;

namespace UniPortal.Pages.Notices
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class NoticeModel : BasePageModel
    {
        private readonly NoticeService _noticeService;

        public NoticeModel(NoticeService notificationService, AccountService accountService) : base(accountService)
        {
            _noticeService = notificationService;
        }

        public List<Notice> Notices { get; set; } = new();
        public List<RecipientType> RecipientTypes { get; set; } = new();

        [BindProperty] public Notice NewNotice { get; set; } = new();
        [BindProperty] public Notice EditNotice { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditNoticeId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            RecipientTypes = await _noticeService.GetNotificationTypesAsync();
            var allNotifications = await _noticeService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allNotifications = allNotifications
                    .Where(n => n.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allNotifications.Count / (double)PageSize);
            Notices = allNotifications
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _noticeService.CreateAsync(
                NewNotice.Title,
                NewNotice.Message,
                CurrentAccount.Id,
                NewNotice.RecipientTypeId,
                NewNotice.RecipientId
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditNoticeId = id;
            var notif = await _noticeService.GetByIdAsync(id);
            if (notif != null)
            {
                EditNotice = new Notice
                {
                    Id = notif.Id,
                    Title = notif.Title,
                    Message = notif.Message,
                    RecipientTypeId = notif.RecipientTypeId,
                    RecipientId = notif.RecipientId,
                    UpdatedAt = DateTime.Now,
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditNoticeId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _noticeService.UpdateAsync(
                Guid.Parse(id),
                EditNotice.Title,
                EditNotice.Message,
                EditNotice.RecipientTypeId,
                EditNotice.RecipientId
            );
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _noticeService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _noticeService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
