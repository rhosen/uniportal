using UniPortal.Services;

namespace UniPortal.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            // Add our Identity related services
            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<Services.Admin.AdminDashboardService>();
            services.AddScoped<Services.Faculty.FacultyDashboardService>();
            services.AddScoped<Services.Faculty.TeacherService>();
            services.AddScoped<DepartmentService>();

            return services;
        }
    }
}
