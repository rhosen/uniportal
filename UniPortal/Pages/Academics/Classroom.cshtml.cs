using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Pages.Academics
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class ClassroomModel : PageModel
    {
        private readonly ClassroomService _classroomService;

        public ClassroomModel(ClassroomService classroomService)
        {
            _classroomService = classroomService;
        }

        public List<Classroom> Classrooms { get; set; } = new();

        [BindProperty] public Classroom NewClassroom { get; set; } = new();
        [BindProperty] public Classroom EditClassroom { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditClassroomId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            var allClassrooms = await _classroomService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allClassrooms = allClassrooms
                    .Where(c => c.RoomName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allClassrooms.Count / (double)PageSize);
            Classrooms = allClassrooms
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            await _classroomService.CreateAsync(NewClassroom.RoomName, NewClassroom.Capacity, NewClassroom.Location);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditClassroomId = id;
            var classroom = await _classroomService.GetByIdAsync(id);
            if (classroom != null)
            {
                EditClassroom = new Classroom
                {
                    Id = classroom.Id,
                    RoomName = classroom.RoomName,
                    Capacity = classroom.Capacity,
                    Location = classroom.Location,
                };
            }
            await OnGetAsync();
            return Page();
        }

        public IActionResult OnPostCancelEdit()
        {
            EditClassroomId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            await _classroomService.UpdateAsync(Guid.Parse(id), EditClassroom.RoomName, EditClassroom.Capacity, EditClassroom.Location);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _classroomService.DeleteAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            await _classroomService.ActivateAsync(id);
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
