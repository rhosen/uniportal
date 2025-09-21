using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var now = DateTime.UtcNow;

            // --- Roles ---
            var roles = new[] { Roles.Admin, Roles.Root };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- Admin User ---
            var adminEmail = "admin@uniportal.com";
            var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, Passwords.Admin);
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);

            dbContext.Accounts.Add(new Account
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = adminEmail,
                IdentityId = adminUser.Id,
                IsActive = true,
                Gender = "Other",
                CreatedAt = now
            });

            // --- Root User ---
            var rootEmail = "root@uniportal.com";
            var rootUser = new IdentityUser { UserName = rootEmail, Email = rootEmail, EmailConfirmed = true };
            await userManager.CreateAsync(rootUser, Passwords.Root);
            await userManager.AddToRoleAsync(rootUser, Roles.Root);

            dbContext.Accounts.Add(new Account
            {
                FirstName = "Root",
                LastName = "",
                Email = rootEmail,
                IdentityId = rootUser.Id,
                IsActive = true,
                Gender = "Other",
                CreatedAt = now
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
