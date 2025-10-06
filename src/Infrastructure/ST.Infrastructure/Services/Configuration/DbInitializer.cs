using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Seeds;

namespace ST.Infrastructure.Services.Configuration
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ISettingSeeder _settingSeeder;

        public DbInitializer(ISettingSeeder settingSeeder)
        {
            _settingSeeder = settingSeeder;
        }

        public async Task InitializeAsync()
        {
            await _settingSeeder.SeedAsync();
        }
    }
}