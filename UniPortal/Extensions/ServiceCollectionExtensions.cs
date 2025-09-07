using UniPortal.Services;
using UniPortal.Services.Admin;
using UniPortal.Services.Faculty;
using UniPortal.Services.Student;

namespace UniPortal.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<AdminDashboardService>();
            services.AddScoped<AdminService>();


            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<TeacherService>();
            services.AddScoped<DepartmentService>();
            services.AddScoped<SemesterService>();
            services.AddScoped<CourseService>();
            services.AddScoped<ClassroomService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<SubjectService>();
            services.AddScoped<ClassScheduleService>();
            services.AddSingleton<FileLogService>();
            services.AddScoped<LogService>();
            services.AddScoped<StudentService>();

            services.AddScoped<FacultyDashboardService>();

            services.AddScoped<StudentDashboardService>();
            services.AddScoped<AssignmentService>();



            return services;
        }
    }
}
