using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Services;

namespace UniPortal.Pages.Admin
{
    public class StudentsModel : PageModel
    {

        private readonly AccountService _accountService;

        public StudentsModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        public List<Data.Entities.Account> ActiveStudents { get; set; } = new();

        [BindProperty]
        public Guid StudentId { get; set; }

        public async Task OnGetAsync()
        {
            ActiveStudents = await _accountService.GetActiveStudentsAsync();
        }
    }
}
