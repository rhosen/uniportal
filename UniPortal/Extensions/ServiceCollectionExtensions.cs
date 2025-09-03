namespace UniPortal.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            // Add our Identity related services
            services.AddScoped<Services.UserService>();
            services.AddScoped<Services.AccountService>();
            services.AddScoped<Services.Admin.AdminDashboardService>();
            services.AddScoped<Services.Faculty.FacultyDashboardService>();

            return services;
        }
    }
}
