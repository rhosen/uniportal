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
            // --- Initializers ---
            services.AddScoped<AppInitializer>();

            // --- Unit of Work ---
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var context = provider.GetRequiredService<UniPortalContext>();
                return new UnitOfWork(context);
            });

            // --- User & Account Services ---
            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<TeacherService>();
            services.AddScoped<StudentService>();
            services.AddScoped<AdminService>();

            // --- Dashboards ---
            services.AddScoped<AdminDashboardService>();
            services.AddScoped<FacultyDashboardService>();
            services.AddScoped<StudentDashboardService>();

            // --- Academic Config Services ---
            services.AddScoped<DepartmentService>();
            services.AddScoped<SemesterService>();
            services.AddScoped<ClassroomService>();
            services.AddScoped<SubjectService>();
            services.AddScoped<NoticeService>();
            services.AddScoped<ProgramService>(); 

            // --- Academic Operations Services ---
            services.AddScoped<CourseService>();
            services.AddScoped<ClassScheduleService>();
            services.AddScoped<AssignmentService>();
            services.AddScoped<GradeService>();
            services.AddScoped<EnrollmentService>();

            // --- Infrastructure & Logging ---
            services.AddSingleton<FileLogService>();
            services.AddScoped<LogService>();

            // --- Utilities ---
            services.AddScoped<StudentIdGenerator>();

            return services;
        }
    }
}
