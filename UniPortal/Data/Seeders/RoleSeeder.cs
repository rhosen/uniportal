using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;

namespace UniPortal.Data.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Add new roles including Root
            string[] roles = { Roles.Student, Roles.Faculty, Roles.Admin, Roles.Root };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
