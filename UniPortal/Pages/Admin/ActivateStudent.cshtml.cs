using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services;

namespace UniPortal.Pages.Admin
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class ActivateStudentModel : PageModel
    {
        private readonly AccountService _accountService;

        public ActivateStudentModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        public List<Data.Entities.Account> InactiveStudents { get; set; } = new();

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        [BindProperty] public Guid AccountId { get; set; }

        public async Task OnGetAsync()
        {
            var allStudents = await _accountService.GetInactiveStudentsAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allStudents = allStudents
                    .Where(s => (s.FirstName + " " + s.LastName)
                                .Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allStudents.Count / (double)PageSize);
            InactiveStudents = allStudents
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        // Activate account
        public async Task<IActionResult> OnPostActivateAsync()
        {
            if (AccountId != Guid.Empty)
                await _accountService.ActivateAsync(AccountId);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
