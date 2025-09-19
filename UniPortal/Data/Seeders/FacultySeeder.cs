using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class FacultySeeder
    {
        public static async Task SeedFacultyAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = services.GetRequiredService<UniPortalContext>();

            // Ensure Faculty role exists
            if (!await roleManager.RoleExistsAsync(Roles.Faculty))
                await roleManager.CreateAsync(new IdentityRole(Roles.Faculty));

            // List of default faculty
            var faculties = new List<(string FirstName, string LastName, string Email)>
            {
                ("John", "Doe", "john.doe@uniportal.com"),
                ("Alice", "Smith", "alice.smith@uniportal.com"),
                ("Robert", "Johnson", "robert.johnson@uniportal.com"),
                ("Emma", "Williams", "emma.williams@uniportal.com")
            };

            foreach (var (firstName, lastName, email) in faculties)
            {
                // Check if IdentityUser exists
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userManager.CreateAsync(user, Passwords.Teacher); // default password
                    await userManager.AddToRoleAsync(user, Roles.Faculty);
                }

                // Check if corresponding Account exists
                if (!dbContext.Accounts.Any(a => a.IdentityId == user.Id))
                {
                    dbContext.Accounts.Add(new Account
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        IdentityId = user.Id,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
