using Microsoft.AspNetCore.Authorization;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Portals
{
    [Authorize]
    public class RoomModel : BasePageModel
    {
        private readonly RoomService _classroomService;

        public List<ClassroomStatusDto> Classrooms { get; set; } = new();

        public RoomModel(RoomService classroomService, AccountService accountService) : base(accountService)
        {
            _classroomService = classroomService;
        }

        public async Task OnGetAsync()
        {
            Classrooms = await _classroomService.GetClassroomStatusAsync();
        }
    }
}
