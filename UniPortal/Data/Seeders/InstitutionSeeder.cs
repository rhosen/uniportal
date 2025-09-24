using UniPortal.Data.Entities;

namespace UniPortal.Data.Seeders
{
    public class InstitutionSeeder
    {
        public static async Task SeedAsync(UniPortalContext dbContext)
        {
            var now = DateTime.Now;

            var institution = new Institution
            {
                Id = Guid.NewGuid(),
                Name = "City University",
                Address = "Khagan, Ashulia, Birulia, Savar, Dhaka-1340, Bangladesh",
                Email = "admin@cityuniversity.ac.bd",
                Phone = "+8801322917670",
                LogoUrl = "/img/logo.png",
                CreatedAt = now,
                IsDeleted = false
            };

            dbContext.Institutions.Add(institution);
            await dbContext.SaveChangesAsync();
        }
    }
}
