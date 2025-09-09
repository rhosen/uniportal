using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Academics;

namespace UniPortal.Pages.Classes
{
    public class RoomModel : BasePageModel
    {
        private readonly ClassroomService _classroomService;

        public List<ClassroomAvailabilityViewModel> Classrooms { get; set; } = new();

        public RoomModel(ClassroomService classroomService, AccountService accountService) : base(accountService)
        {
            _classroomService = classroomService;
        }

        public async Task OnGetAsync()
        {
            Classrooms = await _classroomService.GetClassroomAvailabilityAsync();
        }
    }
}
