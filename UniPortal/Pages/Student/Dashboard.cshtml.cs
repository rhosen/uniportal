using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using UniPortal.Constants;
using UniPortal.Services;

namespace UniPortal.Pages.Student
{
    [Authorize(Roles = Roles.Student)]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AccountService _accountService;
        private readonly UserService _userService;

        public DashboardModel(UserManager<IdentityUser> userManager,
                              AccountService accountService,
                              UserService userService)
        {
            _userManager = userManager;
            _accountService = accountService;
            _userService = userService;
        }

        // -------------------------
        // Dashboard data (mocked for now)
        // -------------------------
        public List<ClassInfo> UpcomingClasses { get; set; } = new();
        public List<AssignmentInfo> PendingAssignments { get; set; } = new();
        public List<ClassroomInfo> Classrooms { get; set; } = new();
        public List<GradeInfo> PastGrades { get; set; } = new();

        [BindProperty]
        public Guid AssignmentId { get; set; }  // For submission handler

        // -------------------------
        // Profile section
        // -------------------------
        [BindProperty]
        public ProfileInputModel Profile { get; set; } = new();

        public class ProfileInputModel
        {
            [Required, MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required, MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress, MaxLength(100)]
            public string Email { get; set; } = string.Empty;

            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [MaxLength(20)]
            public string PhoneNumber { get; set; } = string.Empty;

            [MaxLength(100)]
            public string Address { get; set; } = string.Empty;
        }

        // -------------------------
        // Password section
        // -------------------------
        [BindProperty]
        public ChangePasswordInputModel ChangePasswordModel { get; set; } = new();

        public class ChangePasswordInputModel
        {
            [Required, DataType(DataType.Password)]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), MinLength(6)]
            public string NewPassword { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        // -------------------------
        // GET
        // -------------------------
        public async Task<IActionResult> OnGetAsync()
        {
            LoadMockData();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var account = await _accountService.GetByUserIdAsync(user.Id);
            if (account != null)
            {
                Profile = new ProfileInputModel
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    PhoneNumber = account.PhoneNumber,
                    Address = account.Address
                };
            }

            return Page();
        }

        // -------------------------
        // Submit assignment
        // -------------------------
        public IActionResult OnPostSubmitAssignment()
        {
            var assignment = PendingAssignments.FirstOrDefault(a => a.Id == AssignmentId);
            if (assignment != null)
            {
                PendingAssignments.Remove(assignment);
                TempData["SuccessMessage"] = $"Assignment '{assignment.Title}' submitted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Assignment not found.";
            }

            LoadMockData();
            return RedirectToPage();
        }

        // -------------------------
        // Update profile
        // -------------------------
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var success = await _accountService.UpdateProfileAsync(
                user.Id,
                Profile.FirstName,
                Profile.LastName,
                Profile.PhoneNumber,
                Profile.Address
            );

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Failed to update profile.");
                return Page();
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToPage();
        }

        // -------------------------
        // Change password
        // -------------------------
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var result = await _userService.ChangePasswordAsync(
                user,
                ChangePasswordModel.CurrentPassword,
                ChangePasswordModel.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToPage();
        }

        // -------------------------
        // Mock Data Loader
        // -------------------------
        private void LoadMockData()
        {
            UpcomingClasses = new List<ClassInfo>
            {
                new ClassInfo { CourseName = "Math 101", Time = "10:00 - 11:30", Room = "A1" },
                new ClassInfo { CourseName = "Physics 201", Time = "12:00 - 13:30", Room = "B2" }
            };

            PendingAssignments = new List<AssignmentInfo>
            {
                new AssignmentInfo { Id = Guid.NewGuid(), Title = "Math Homework 1", DueDate = DateTime.Now.AddDays(2) },
                new AssignmentInfo { Id = Guid.NewGuid(), Title = "Physics Lab Report", DueDate = DateTime.Now.AddDays(4) }
            };

            Classrooms = new List<ClassroomInfo>
            {
                new ClassroomInfo { RoomName = "A1", IsOccupied = false },
                new ClassroomInfo { RoomName = "B2", IsOccupied = true },
                new ClassroomInfo { RoomName = "C3", IsOccupied = false }
            };

            PastGrades = new List<GradeInfo>
            {
                new GradeInfo { Semester = "Fall 2024", Subject = "Math 101", Grade = "A" },
                new GradeInfo { Semester = "Fall 2024", Subject = "Physics 201", Grade = "B+" },
                new GradeInfo { Semester = "Spring 2025", Subject = "Chemistry 101", Grade = "A-" }
            };
        }
    }

    // Supporting classes
    public class ClassInfo
    {
        public string CourseName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
    }

    public class AssignmentInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    public class ClassroomInfo
    {
        public string RoomName { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
    }

    public class GradeInfo
    {
        public string Semester { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
    }
}
