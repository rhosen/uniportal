using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.ViewModel;

namespace UniPortal.Services.Admin
{
    public class AdminDashboardService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UniPortalContext _context;

        public AdminDashboardService(UniPortalContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminProfileViewModel?> GetAdminProfileAsync(Guid accountId)
        {
            return await _context.Accounts
                .Where(a => a.Id == accountId && !a.IsDeleted && a.IsActive)
                .Select(a => new AdminProfileViewModel
                {
                    Id = a.Id,
                    FullName = a.FirstName + " " + a.LastName,
                    Email = a.Email,
                    Phone = a.Phone
                })
                .FirstOrDefaultAsync();
        }

        // Students
        public async Task<int> GetTotalStudentsAsync(bool? isActive = null)
        {
            var studentIds = (await _userManager.GetUsersInRoleAsync(Roles.Student))
                .Select(u => u.Id)
                .ToList();

            var query = _context.Accounts
                .Where(a => studentIds.Contains(a.IdentityUserId) && !a.IsDeleted);

            if (isActive.HasValue)
                query = query.Where(a => a.IsActive == isActive.Value);

            return await query.CountAsync();
        }

        // Teachers
        public async Task<int> GetTotalTeachersAsync(bool? isActive = null)
        {
            var teacherIds = (await _userManager.GetUsersInRoleAsync(Roles.Faculty))
                .Select(u => u.Id)
                .ToList();

            var query = _context.Accounts
                .Where(a => teacherIds.Contains(a.IdentityUserId) && !a.IsDeleted);

            if (isActive.HasValue)
                query = query.Where(a => a.IsActive == isActive.Value);

            return await query.CountAsync();
        }

        // Courses
        public async Task<int> GetTotalCoursesAsync()
        {
            return await _context.Courses.CountAsync(c => !c.IsDeleted);
        }

       
    }
}
