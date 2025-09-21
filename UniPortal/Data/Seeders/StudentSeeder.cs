using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class StudentSeeder
    {
        public static async Task SeedStudentsAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = services.GetRequiredService<UniPortalContext>();
            var now = DateTime.UtcNow;

            // Fetch a default program, batch, section
            var defaultProgram = await dbContext.Programs.FirstOrDefaultAsync();
            var defaultBatch = await dbContext.Batches.FirstOrDefaultAsync();
            var defaultSection = await dbContext.Sections.FirstOrDefaultAsync();

            if (defaultProgram == null || defaultBatch == null || defaultSection == null)
                throw new Exception("Program, Batch or Section missing. Seed them first.");

            // List of default students
            var students = new List<(string FirstName, string LastName, string StudentNumber)>
            {
                ("Michael", "Brown", "STU001"),
                ("Sarah", "Davis", "STU002"),
                ("David", "Miller", "STU003"),
                ("Emily", "Wilson", "STU004")
            };

            foreach (var (firstName, lastName, studentNumber) in students)
            {
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}@uniportal.com";

                // Skip if account already exists
                if (dbContext.Accounts.Any(a => a.Email == email)) continue;

                // Create IdentityUser
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, Passwords.Student); // <- same pattern as FacultySeeder
                await userManager.AddToRoleAsync(user, Roles.Student);

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

                // Create corresponding Student
                var student = new Student
                {
                    AccountId = account.Id,
                    StudentNumber = studentNumber,
                    ProgramId = defaultProgram.Id,
                    BatchId = defaultBatch.Id,
                    SectionId = defaultSection.Id,
                    CurrentSemester = 1,
                    CreatedAt = now
                };
                dbContext.Students.Add(student);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
