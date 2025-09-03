using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services;

namespace UniPortal.Pages.Admin
{
    public class ActivateStudentsModel : PageModel
    {
        private readonly AccountService _accountService;

        public ActivateStudentsModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        public List<Data.Entities.Account> InactiveStudents { get; set; } = new();

        [BindProperty]
        public Guid StudentId { get; set; }

        public async Task OnGetAsync()
        {
            InactiveStudents = await _accountService.GetInactiveStudentsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (StudentId != Guid.Empty)
            {
                await _accountService.ActivateStudentAsync(StudentId);
            }
            return RedirectToPage();
        }
    }
}
