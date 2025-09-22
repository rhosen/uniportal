using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class RoomModel : BasePageModel
    {
        private readonly RoomService _roomService;

        public RoomModel(RoomService roomService,
            AccountService accountService) : base(accountService)
        {
            _roomService = roomService;
        }

        public List<Room> Classrooms { get; set; } = new();

        [BindProperty] public Room NewClassroom { get; set; } = new();
        [BindProperty] public Room EditClassroom { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? EditClassroomId { get; set; }

        // Search & Pagination
        [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch all non-deleted rooms
            var allRooms = await _roomService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allRooms = allRooms
                    .Where(r => r.RoomName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allRooms.Count / (double)PageSize);
            Classrooms = allRooms
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewClassroom.RoomName))
            {
                await _roomService.CreateAsync(NewClassroom.RoomName, NewClassroom.Capacity, NewClassroom.Location, isClassroom: true);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditClassroomId = id;
            if (!Guid.TryParse(id, out var classroomId))
                return RedirectToPage();

            var classroom = await _roomService.GetByIdAsync(classroomId);
            if (classroom != null)
            {
                EditClassroom = new Room
                {
                    Id = classroom.Id,
                    RoomName = classroom.RoomName,
                    Capacity = classroom.Capacity,
                    Location = classroom.Location,
                    IsClassroom = classroom.IsClassroom
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
            if (!Guid.TryParse(id, out var classroomId))
                return RedirectToPage(new { CurrentPage, SearchTerm });

            await _roomService.UpdateAsync(
                classroomId,
                EditClassroom.RoomName,
                EditClassroom.Capacity,
                EditClassroom.Location,
                EditClassroom.IsClassroom // keep the classroom flag consistent
            );

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (Guid.TryParse(id, out var classroomId))
            {
                await R(() => _roomService.DeleteAsync(classroomId), "Room deleted successfully.");
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (Guid.TryParse(id, out var classroomId))
            {
                await _roomService.ActivateAsync(classroomId);
            }
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
