using UniPortal.Data;
using UniPortal.Helpers;
using UniPortal.Services.Academics.Configs;
using UniPortal.Services.Academics.Operations;
using UniPortal.Services.Accounts;
using UniPortal.Services.Dashboards;
using UniPortal.Services.Infrastructures;
using UniPortal.Services.Notices;

namespace UniPortal.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<AppInitializer>();

            services.AddScoped<IUnitOfWork>(provider =>
            {
                var context = provider.GetRequiredService<UniPortalContext>();
                return new UnitOfWork(context);
            });

            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<TeacherService>();
            services.AddScoped<StudentService>();
            services.AddScoped<AdminService>();

            services.AddScoped<AdminDashboardService>();
            services.AddScoped<FacultyDashboardService>();
            services.AddScoped<StudentDashboardService>();

            services.AddScoped<DepartmentService>();
            services.AddScoped<SemesterService>();
            services.AddScoped<ClassroomService>();
            services.AddScoped<NoticeService>();
            services.AddScoped<SubjectService>();

            services.AddScoped<CourseService>();
            services.AddScoped<ClassScheduleService>();
            services.AddScoped<AssignmentService>();

            services.AddSingleton<FileLogService>();
            services.AddScoped<LogService>();

            services.AddScoped<StudentIdGenerator>();

            return services;
        }
    }
}
