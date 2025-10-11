using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ST.Infrastructure.Persistence.Contexts;
using System.IO;

namespace ST.Infrastructure.Persistence
{

    public class SharedDbContextFactory : IDesignTimeDbContextFactory<SharedDbContext>
    {
        public SharedDbContext CreateDbContext(string[] args)
        {
            string basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Presentation", "ST.App"));

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The 'DefaultConnection' connection string was not found in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<SharedDbContext>();

            optionsBuilder.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(SharedDbContext).Assembly.FullName));

            return new SharedDbContext(optionsBuilder.Options);
        }
    }
}
