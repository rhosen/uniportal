using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data;
using UniPortal.ViewModels.Users;

namespace UniPortal.Services.Dashboards
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

        public async Task<AdminProfileViewModel> GetAdminProfileAsync(Guid accountId)
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
            // Start query joining Accounts with Students
            var query = from account in _context.Accounts
                        join student in _context.Students
                            on account.Id equals student.AccountId
                        where !account.IsDeleted
                        select account;

            // Filter by IsActive if specified
            if (isActive.HasValue)
            {
                query = query.Where(a => a.IsActive == isActive.Value);
            }

            return await query.CountAsync();
        }

        public async Task<int> GetPendingStudentsAsync()
        {
            var query = from account in _context.Accounts
                        where account.IsActive
                              && !account.IsDeleted
                              && !_context.Students.Any(s => s.AccountId == account.Id)
                        join userRole in _context.UserRoles on account.IdentityUserId equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        where role.Name == Roles.Student
                        select account;

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
