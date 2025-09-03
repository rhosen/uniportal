using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Data;

namespace UniPortal.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddCustomIdentity(this IServiceCollection services, string connectionString)
        {
            // Add DB context
            services.AddDbContext<UniPortalContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Identity
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false; // optional for MVP
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<UniPortalContext>()
                .AddDefaultTokenProviders();
            return services;
        }

        // Seed roles (call at app startup)
        public static async Task SeedRolesAsync(this IServiceProvider serviceProvider, params string[] roles)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
