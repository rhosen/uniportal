using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text;
using UniPortal.Data;
using UniPortal.Data.Seeders;

namespace UniPortal.Services.Infrastructures
{
    public class AppInitializer
    {
        private readonly UniPortalContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AppInitializer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AppInitializer(UniPortalContext context, IWebHostEnvironment env, ILogger<AppInitializer> logger, IServiceProvider serviceProvider)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            var databaseCreator = _context.Database.GetService<IRelationalDatabaseCreator>();

            if (!await databaseCreator.ExistsAsync())
            {
                _logger.LogInformation("Database does not exist. Creating database...");

                // 1️⃣ Create the database
                await databaseCreator.CreateAsync();
                _logger.LogInformation("Database created successfully.");

                // 2️⃣ Execute SQL scripts
                await RunScriptsAsync();

                // 3️⃣ Run your seeders
                await RunSeedersAsync();

                _logger.LogInformation("First-time initialization completed.");
            }
            else
            {
                _logger.LogInformation("Database already exists. Initialization skipped.");
            }
        }
        private async Task RunScriptsAsync()
        {
            var scriptsFolder = Path.Combine(_env.ContentRootPath, "Data", "Scripts");
            string[] scripts = { "identity.sql", "schema.sql" }; // adjust order

            foreach (var scriptName in scripts)
            {
                var scriptPath = Path.Combine(scriptsFolder, scriptName);
                if (!File.Exists(scriptPath))
                {
                    _logger.LogWarning($"Script not found: {scriptName}");
                    continue;
                }

                _logger.LogInformation($"Executing script: {scriptName}");

                var lines = await File.ReadAllLinesAsync(scriptPath);
                var batch = new StringBuilder();
                int batchNumber = 1;

                foreach (var line in lines)
                {
                    if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                    {
                        var batchText = batch.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(batchText))
                        {
                            _logger.LogInformation($"Executing batch {batchNumber} from script {scriptName}...");
                            await _context.Database.ExecuteSqlRawAsync(batchText);
                            _logger.LogInformation($"Batch {batchNumber} executed successfully.");
                            batchNumber++;
                        }
                        batch.Clear();
                    }
                    else
                    {
                        batch.AppendLine(line);
                    }
                }

                // Execute remaining batch
                var remaining = batch.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(remaining))
                {
                    _logger.LogInformation($"Executing final batch from script {scriptName}...");
                    await _context.Database.ExecuteSqlRawAsync(remaining);
                    _logger.LogInformation($"Final batch executed successfully.");
                }

                _logger.LogInformation($"Script executed successfully: {scriptName}");
            }
        }

        private async Task RunSeedersAsync()
        {
            _logger.LogInformation("Running seeders...");

            using var scope = _serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await RoleSeeder.SeedRolesAsync(roleManager);
            await AdminSeeder.SeedAdminAsync(scope.ServiceProvider);
            await FacultySeeder.SeedFacultyAsync(scope.ServiceProvider);
            await RecipientTypeSeeder.SeedRecipientTypesAsync(scope.ServiceProvider);

            _logger.LogInformation("Seeders executed successfully.");
        }
    }

}


