using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ST.Infrastructure.Persistence.Contexts; // Kendi DbContext namespace'inizi doğrulayın
using System.IO;

namespace ST.Infrastructure.Persistence
{

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Proje yapınıza göre başlangıç projesinin (ST.App) yolunu doğru bir şekilde bulalım.
            // Bu kod, ST.Infrastructure klasöründen iki seviye yukarı çıkıp
            // Presentation/ST.App klasörüne girmeyi hedefler.
            // Yol: Saticiyiz/src/Infrastructure/ST.Infrastructure -> ../../ -> Saticiyiz/src/ -> Presentation/ST.App
            string basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Presentation", "ST.App"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The 'DefaultConnection' connection string was not found in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            // Sadece DbContextOptions alan kurucuyu (constructor) çağırarak
            // tasarım zamanında oluşabilecek DI (Dependency Injection) hatalarını önlüyoruz.
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}