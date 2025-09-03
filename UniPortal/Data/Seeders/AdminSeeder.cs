using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = services.GetRequiredService<UniPortalContext>();

            // 1️⃣ Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

            // 2️⃣ Create admin user if not exists
            var adminEmail = "admin@uniportal.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin@123"); // default password
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }

            // 3️⃣ Create corresponding Account entry
            if (!dbContext.Accounts.Any(a => a.IdentityUserId == adminUser.Id))
            {
                dbContext.Accounts.Add(new Account
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = adminEmail,
                    IdentityUserId = adminUser.Id,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                await dbContext.SaveChangesAsync();
            }
        }

    }
}
