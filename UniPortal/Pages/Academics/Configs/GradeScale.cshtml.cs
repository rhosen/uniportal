using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Accounts;

namespace UniPortal.Pages.Academics.Configs
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Root)]
    public class GradeScaleModel : BasePageModel
    {
        private readonly GradeScaleService _gradeScaleService;

        public GradeScaleModel(GradeScaleService gradeScaleService,
                               AccountService accountService) : base(accountService)
        {
            _gradeScaleService = gradeScaleService;
        }

        public List<GradeScale> GradeScales { get; set; } = new();

        [BindProperty] public string? EditGradeScaleId { get; set; }
        [BindProperty] public GradeScale EditGradeScale { get; set; } = new();
        [TempData] public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            GradeScales = await _gradeScaleService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditGradeScaleId = id;
            if (!Guid.TryParse(id, out var scaleId))
                return RedirectToPage();

            var scale = await _gradeScaleService.GetByIdAsync(scaleId);
            if (scale != null)
            {
                EditGradeScale = new GradeScale
                {
                    Id = scale.Id,
                    Grade = scale.Grade,
                    MinMarks = scale.MinMarks,
                    MaxMarks = scale.MaxMarks,
                    GPA = scale.GPA
                };
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelEdit()
        {
            EditGradeScaleId = null;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!Guid.TryParse(id, out var scaleId))
                return RedirectToPage();

            try
            {
                await _gradeScaleService.UpdateAsync(
                    scaleId,
                    EditGradeScale.Grade,
                    EditGradeScale.MinMarks,
                    EditGradeScale.MaxMarks,
                    EditGradeScale.GPA
                );
                EditGradeScaleId = null;
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }

            await OnGetAsync();
            return Page();
        }
    }
}
