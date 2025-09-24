using UniPortal.Data;
using UniPortal.Helpers;
using UniPortal.Services.Academics;
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
            // ========================
            // Initializers
            // ========================
            services.AddScoped<AppInitializer>();

            // ========================
            // Unit of Work
            // ========================
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var context = provider.GetRequiredService<UniPortalContext>();
                return new UnitOfWork(context);
            });

            // ========================
            // User & Account Services
            // ========================
            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<FacultyService>();
            services.AddScoped<StudentService>();
            services.AddScoped<AdminService>();

            // ========================
            // Dashboards
            // ========================
            services.AddScoped<AdminDashboardService>();
            services.AddScoped<FacultyDashboardService>();
            services.AddScoped<StudentDashboardService>();

            // ========================
            // Academic Config Services
            // ========================
            services.AddScoped<DepartmentService>();
            services.AddScoped<SemesterService>();
            services.AddScoped<RoomService>();
            services.AddScoped<CourseService>();
            services.AddScoped<CourseTypeService>();
            services.AddScoped<NoticeService>();
            services.AddScoped<ProgramService>();
            services.AddScoped<DegreeService>();
            services.AddScoped<SectionService>();
            services.AddScoped<BatchService>();
            services.AddScoped<CurriculumService>();
            services.AddScoped<GradeScaleService>(); 
            services.AddScoped<RecipientService>(); 


            // ========================
            // Academic Operations Services
            // ========================
            services.AddScoped<CourseOfferingService>();
            services.AddScoped<AssignmentService>();
            services.AddScoped<GradeService>();
            services.AddScoped<EnrollmentService>();

            // ========================
            // Infrastructure & Logging
            // ========================
            services.AddSingleton<FileLogService>(); // singleton for file-based logging
            services.AddScoped<LogService>();        // scoped for request-specific logging

            // ========================
            // Utilities
            // ========================
            services.AddScoped<StudentIdGenerator>();
            services.AddScoped<FacultyNumberGenerator>();

            return services;
        }
    }
}
