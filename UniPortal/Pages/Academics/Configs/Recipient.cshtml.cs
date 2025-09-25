using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class RecipientModel : BasePageModel
    {
        private readonly RecipientService _recipientService;

        public RecipientModel(RecipientService recipientService, AccountService accountService) : base(accountService)
        {
            _recipientService = recipientService;
        }

        public List<Data.Entities.RecipientType> Recipients { get; set; } = new();

        [BindProperty] public Data.Entities.RecipientType NewRecipient { get; set; } = new();
        [BindProperty] public Data.Entities.RecipientType EditRecipient { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditRecipientId { get; set; }

        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var all = await _recipientService.GetAllAsync();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                all = all
                    .Where(r => r.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
            Recipients = all
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewRecipient.Name))
                await _recipientService.CreateAsync(NewRecipient.Name, NewRecipient.Description);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditRecipientId = id;
            if (!Guid.TryParse(id, out var recId)) return RedirectToPage();

            var rec = await _recipientService.GetByIdAsync(recId);
            if (rec != null)
            {
                EditRecipient = new Data.Entities.RecipientType
                {
                    Id = rec.Id,
                    Name = rec.Name,
                    Description = rec.Description
                };
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditRecipientId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var recId)) return RedirectToPage(new { CurrentPage, SearchTerm });

            await _recipientService.UpdateAsync(recId, EditRecipient.Name, EditRecipient.Description);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var recId))
                await R(() => _recipientService.DeleteAsync(recId), "Recipient deleted successfully.");

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var recId))
                await _recipientService.ActivateAsync(recId);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
