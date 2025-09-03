using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class FacultySeeder
    {
        public static async Task SeedFacultyAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = services.GetRequiredService<UniPortalContext>();

            // Ensure Teacher role exists
            if (!await roleManager.RoleExistsAsync(Roles.Faculty))
                await roleManager.CreateAsync(new IdentityRole(Roles.Faculty));

            // Create default faculty user
            var facultyEmail = "teacher@uniportal.com";
            var facultyUser = await userManager.FindByEmailAsync(facultyEmail);
            if (facultyUser == null)
            {
                facultyUser = new IdentityUser { UserName = facultyEmail, Email = facultyEmail, EmailConfirmed = true };
                await userManager.CreateAsync(facultyUser, "Teacher@123"); // default password
                await userManager.AddToRoleAsync(facultyUser, Roles.Faculty);
            }

            // Create corresponding Account entry
            if (!dbContext.Accounts.Any(a => a.IdentityUserId == facultyUser.Id))
            {
                dbContext.Accounts.Add(new Account
                {
                    FirstName = "Default",
                    LastName = "Teacher",
                    Email = facultyEmail,
                    IdentityUserId = facultyUser.Id,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }

}
