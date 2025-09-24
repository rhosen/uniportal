using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class StudentSeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext, UserManager<IdentityUser> userManager, List<Data.Entities.Program> programs, List<Batch> batches, List<Section> sections)
        {
            var now = DateTime.Now;
            var studentData = new List<(string FirstName, string LastName, string StudentNumber)>
            {
                ("Liam","Anderson","STU001"),
                ("Olivia","Thomas","STU002"),
                ("Noah","Martin","STU003"),
                ("Emma","Lee","STU004")
            };

            var defaultProgram = programs.First();
            var defaultBatch = batches.First();
            var defaultSection = sections.First();

            foreach (var (firstName, lastName, studentNumber) in studentData)
            {
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}@uniportal.com";
                if (dbContext.Accounts.Any(a => a.Email == email)) continue;

                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                await userManager.CreateAsync(user, Passwords.Student);
                await userManager.AddToRoleAsync(user, Roles.Student);

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
