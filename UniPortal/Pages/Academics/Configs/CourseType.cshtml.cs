using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class CourseTypeModel : BasePageModel
    {
        private readonly CourseTypeService _courseTypeService;

        public CourseTypeModel(CourseTypeService courseTypeService, AccountService accountService)
            : base(accountService)
        {
            _courseTypeService = courseTypeService;
        }

        public List<CourseType> CourseTypes { get; set; } = new();

        [BindProperty] public CourseType NewCourseType { get; set; } = new();
        [BindProperty] public CourseType EditCourseType { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditCourseTypeId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var all = await _courseTypeService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
                all = all.Where(ct => ct.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            TotalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
            CourseTypes = all.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewCourseType.Name))
                await _courseTypeService.CreateAsync(NewCourseType.Name);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditCourseTypeId = id;
            if (!Guid.TryParse(id, out var typeId)) return RedirectToPage();

            var type = await _courseTypeService.GetByIdAsync(typeId);
            if (type != null)
            {
                EditCourseType = new CourseType
                {
                    Id = type.Id,
                    Name = type.Name
                };
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditCourseTypeId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var typeId)) return RedirectToPage(new { CurrentPage, SearchTerm });

            await _courseTypeService.UpdateAsync(typeId, EditCourseType.Name);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var typeId))
                await R(() => _courseTypeService.DeleteAsync(typeId), "Course type deleted successfully.");

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var typeId))
                await _courseTypeService.ActivateAsync(typeId);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
