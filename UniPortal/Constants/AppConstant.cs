
namespace UniPortal.Constants
{
    public static class AppConstant
    {
        public static class Academic
        {
            public const string Department = "Department";
            public const string All = "All";
            public const string Course = nameof(Course);
        }

        public static class AppRoutes
        {
            public const string AdminDashboard = "/dashboards/admin";
            public const string FacultyDashboard = "/dashboards/faculty";
            public const string StudentDashboard = "/dashboards/student";
            public const string Login = "/accounts/login";
            public const string Logout = "/accounts/logout";
        }

        public static class Passwords
        {
            public static string Teacher { get; private set; }
            public static string Admin { get; private set; }
            public static string Root { get; private set; }

            // Method to bind passwords at startup
            public static void LoadPasswords(IConfiguration configuration)
            {
                Teacher = configuration["DefaultPasswords:Teacher"];
                Admin = configuration["DefaultPasswords:Admin"];
                Root = configuration["DefaultPasswords:Root"];
            }
        }
    }
}
