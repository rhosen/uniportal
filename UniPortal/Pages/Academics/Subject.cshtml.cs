using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class SubjectsModel : BasePageModel
    {
        private readonly SubjectService _subjectService;

        public SubjectsModel(SubjectService subjectService, AccountService accountService)
            : base(accountService)
        {
            _subjectService = subjectService;
        }

        public List<Data.Entities.Subject> Subjects { get; set; } = new();

        [BindProperty] public Data.Entities.Subject NewSubject { get; set; } = new();
        [BindProperty] public Data.Entities.Subject EditSubject { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditSubjectId { get; set; }

        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allSubjects = await _subjectService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allSubjects = allSubjects
                    .Where(s => s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || s.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allSubjects.Count / (double)PageSize);
            Subjects = allSubjects
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _subjectService.CreateAsync(NewSubject.Code, NewSubject.Name, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditSubjectId = id;
            var subj = await _subjectService.GetByIdAsync(Guid.Parse(id));
            if (subj != null)
            {
                EditSubject = new Data.Entities.Subject
                {
                    Id = subj.Id,
                    Code = subj.Code,
                    Name = subj.Name
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditSubjectId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _subjectService.UpdateAsync(Guid.Parse(id), EditSubject.Code, EditSubject.Name, CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _subjectService.DeleteAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _subjectService.ActivateAsync(Guid.Parse(id), CurrentAccount.Id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
