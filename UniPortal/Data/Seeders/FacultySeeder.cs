using Microsoft.AspNetCore.Identity;
using UniPortal.Constants;
using UniPortal.Data.Entities;
using static UniPortal.Constants.AppConstant;

namespace UniPortal.Data.Seeders
{
    public class FacultySeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext, UserManager<IdentityUser> userManager, List<Department> departments, List<FacultyType> facultyTypes)
        {
            var now = DateTime.Now;

            var facultyData = new Dictionary<Department, List<(string FirstName, string LastName, string Email, string FacultyNumber, string FacultyTypeName)>>()
            {
                [departments.First(d => d.Code == "CSE")] = new List<(string, string, string, string, string)>
                {
                    ("John","Doe","john.doe@uniportal.com","CSEFAC001","Professor"),
                    ("Alice","Smith","alice.smith@uniportal.com","CSEFAC002","Assistant Professor"),
                    ("Robert","Brown","robert.brown@uniportal.com","CSEFAC003","Lecturer"),
                    ("Emma","Williams","emma.williams@uniportal.com","CSEFAC004","Lecturer")
                },
                [departments.First(d => d.Code == "BBA")] = new List<(string, string, string, string, string)>
                {
                    ("Michael","Johnson","michael.johnson@uniportal.com","BBAFAC001","Professor"),
                    ("Sarah","Davis","sarah.davis@uniportal.com","BBAFAC002","Assistant Professor"),
                    ("David","Miller","david.miller@uniportal.com","BBAFAC003","Lecturer"),
                    ("Emily","Taylor","emily.taylor@uniportal.com","BBAFAC004","Lecturer")
                }
            };

            foreach (var kvp in facultyData)
            {
                var dept = kvp.Key;
                foreach (var (firstName, lastName, email, facultyNumber, typeName) in kvp.Value)
                {
                    if (dbContext.Accounts.Any(a => a.Email == email)) continue;

                    var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userManager.CreateAsync(user, Passwords.Faculty);
                    await userManager.AddToRoleAsync(user, Roles.Faculty);

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

                    var facultyType = facultyTypes.First(ft => ft.Name == typeName);
                    var faculty = new Faculty
                    {
                        AccountId = account.Id,
                        DepartmentId = dept.Id,
                        FacultyTypeId = facultyType.Id,
                        FacultyNumber = facultyNumber,
                        CreatedAt = now
                    };
                    dbContext.Faculties.Add(faculty);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
