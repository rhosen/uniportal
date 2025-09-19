using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Pages.Academics
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class ProgramModel : PageModel
    {
        private readonly ProgramService _programService;
        private readonly DepartmentService _departmentService;

        public ProgramModel(ProgramService programService, DepartmentService departmentService)
        {
            _programService = programService;
            _departmentService = departmentService;
        }

        public List<Data.Entities.Program> Programs { get; set; } = new();
        public List<Department> Departments { get; set; } = new();

        [BindProperty] public Data.Entities.Program NewProgram { get; set; } = new();
        [BindProperty] public Data.Entities.Program EditProgram { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditProgramId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // Load departments for dropdown
            Departments = await _departmentService.GetAllAsync();

            var allPrograms = await _programService.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allPrograms = allPrograms
                    .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                p.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Pagination
            TotalPages = (int)Math.Ceiling(allPrograms.Count / (double)PageSize);
            Programs = allPrograms
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _programService.CreateAsync(NewProgram.Code, NewProgram.Name, NewProgram.DepartmentId);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditProgramId = id;
            var program = await _programService.GetByIdAsync(id);
            if (program != null)
            {
                EditProgram = new Data.Entities.Program
                {
                    Id = program.Id,
                    Code = program.Code,
                    Name = program.Name,
                    DepartmentId = program.DepartmentId,
                };
            }

            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditProgramId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _programService.UpdateAsync(Guid.Parse(id), EditProgram.Code, EditProgram.Name, EditProgram.DepartmentId);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _programService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _programService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
