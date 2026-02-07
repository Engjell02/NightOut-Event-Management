using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Reservation_Management_App.Repository
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Start from where EF runs (bin/Debug/net8.0 usually)
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            // Walk up until we find the Web project folder
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Reservation_Management_App")))
            {
                dir = dir.Parent;
            }

            if (dir == null)
                throw new DirectoryNotFoundException("Could not locate Reservation_Management_App folder by walking up from current directory.");

            var webPath = Path.Combine(dir.FullName, "Reservation_Management_App");
            var appsettingsPath = Path.Combine(webPath, "appsettings.json");

            if (!File.Exists(appsettingsPath))
                throw new FileNotFoundException($"appsettings.json not found at: {appsettingsPath}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(webPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
