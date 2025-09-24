using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class BatchModel : BasePageModel
    {
        private readonly BatchService _batchService;

        public BatchModel(BatchService batchService,
                          AccountService accountService) : base(accountService)
        {
            _batchService = batchService;
        }

        public List<Batch> Batches { get; set; } = new();

        [BindProperty] public Batch NewBatch { get; set; } = new();
        [BindProperty] public Batch EditBatch { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditBatchId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allBatches = await _batchService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allBatches = allBatches
                    .Where(b => b.Number.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allBatches.Count / (double)PageSize);
            Batches = allBatches
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewBatch.Number))
            {
                await _batchService.CreateAsync(NewBatch.Number);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditBatchId = id;
            if (!Guid.TryParse(id, out var batchId)) return RedirectToPage();

            var batch = await _batchService.GetByIdAsync(batchId);
            if (batch != null)
            {
                EditBatch = new Batch
                {
                    Id = batch.Id,
                    Number = batch.Number
                };
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditBatchId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var batchId))
                return RedirectToPage(new { CurrentPage, SearchTerm });

            await _batchService.UpdateAsync(batchId, EditBatch.Number);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var batchId))
            {
                await R(() => _batchService.DeleteAsync(batchId), "Batch deleted successfully.");
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var batchId))
            {
                await _batchService.ActivateAsync(batchId);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
