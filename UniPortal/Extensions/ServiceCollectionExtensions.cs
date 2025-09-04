using UniPortal.Services;
using UniPortal.Services.Faculty;

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
            services.AddScoped<FacultyDashboardService>();
            services.AddScoped<TeacherService>();
            services.AddScoped<DepartmentService>();
            services.AddScoped<SemesterService>();
            services.AddScoped<CourseService>();

            return services;
        }
    }
}
