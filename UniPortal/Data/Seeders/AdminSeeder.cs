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

            // 1️⃣ Ensure roles exist
            string[] roles = { Roles.Admin, Roles.Root };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2️⃣ Create default admin
            var adminEmail = "admin@uniportal.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }

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
            }

            // 3️⃣ Create default root user
            var rootEmail = "root@uniportal.com";
            var rootUser = await userManager.FindByEmailAsync(rootEmail);
            if (rootUser == null)
            {
                rootUser = new IdentityUser { UserName = rootEmail, Email = rootEmail, EmailConfirmed = true };
                await userManager.CreateAsync(rootUser, "Root@123");
                await userManager.AddToRoleAsync(rootUser, Roles.Root);
            }

            if (!dbContext.Accounts.Any(a => a.IdentityUserId == rootUser.Id))
            {
                dbContext.Accounts.Add(new Account
                {
                    FirstName = "Root",
                    LastName = "",
                    Email = rootEmail,
                    IdentityUserId = rootUser.Id,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
