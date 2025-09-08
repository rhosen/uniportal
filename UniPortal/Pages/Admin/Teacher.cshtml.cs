using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services.Faculty;

namespace UniPortal.Pages.Admin
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class TeacherModel : PageModel
    {
        private readonly TeacherService _teacherService;

        public TeacherModel(TeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        public List<Data.Entities.Account> Teachers { get; set; } = new();
        [BindProperty] public Data.Entities.Account NewTeacher { get; set; } = new();
        [BindProperty] public Data.Entities.Account EditTeacher { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditTeacherId { get; set; }

        // Pagination & Search
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allTeachers = await _teacherService.GetAllAsync();

            // Filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allTeachers = allTeachers
                    .Where(t => t.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || t.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || t.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Pagination
            TotalPages = (int)Math.Ceiling(allTeachers.Count / (double)PageSize);
            Teachers = allTeachers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) return Page();

            var password = "Teacher@123!";
            await _teacherService.CreateAsync(NewTeacher.Email, password, NewTeacher.FirstName, NewTeacher.LastName);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditTeacherId = id;
            var teacher = await _teacherService.GetByIdAsync(id);
            if (teacher != null)
            {
                EditTeacher = new Data.Entities.Account
                {
                    Id = teacher.Id,
                    FirstName = teacher.FirstName,
                    LastName = teacher.LastName,
                    Email = teacher.Email
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditTeacherId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!ModelState.IsValid) return Page();

            await _teacherService.UpdateAsync(Guid.Parse(id), EditTeacher.FirstName, EditTeacher.LastName, EditTeacher.Email);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _teacherService.DeleteAsync(Guid.Parse(id));
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _teacherService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
