using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class FacultySeeder
    {
        public static async Task SeedFacultyAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var now = DateTime.UtcNow;

            // --- Create Faculty role if it doesn't exist ---
            if (!await roleManager.RoleExistsAsync(Roles.Faculty))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Faculty));
            }

            // Get a default department (first one in the database)
            var defaultDept = await dbContext.Departments.FirstOrDefaultAsync();
            if (defaultDept == null)
                throw new Exception("No department exists. Please seed departments first.");

            // --- List of default faculty ---
            var faculties = new List<(string FirstName, string LastName, string Email, string FacultyNumber)>
            {
                ("John", "Doe", "john.doe@uniportal.com", "FAC001"),
                ("Alice", "Smith", "alice.smith@uniportal.com", "FAC002"),
                ("Robert", "Johnson", "robert.johnson@uniportal.com", "FAC003"),
                ("Emma", "Williams", "emma.williams@uniportal.com", "FAC004")
            };

            foreach (var (firstName, lastName, email, facultyNumber) in faculties)
            {
                // Skip if account already exists
                if (dbContext.Accounts.Any(a => a.Email == email)) continue;

                // Create IdentityUser
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, Passwords.Faculty);
                await userManager.AddToRoleAsync(user, Roles.Faculty);

                // Create corresponding Account
                var account = new Account
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    IdentityId = user.Id,
                    IsActive = true,
                    Gender = "Other",
                    CreatedAt = now
                };
                dbContext.Accounts.Add(account);

                // Create corresponding Faculty
                var faculty = new Faculty
                {
                    AccountId = account.Id,
                    FacultyNumber = facultyNumber,
                    DepartmentId = defaultDept.Id, // assign default department
                    CreatedAt = now
                };
                dbContext.Faculties.Add(faculty);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
