using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Seeds;

namespace ST.Infrastructure.Services.Configuration
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ISettingSeeder _settingSeeder;
        private readonly IPlanSeeder _planSeeder;

        public DbInitializer(ISettingSeeder settingSeeder, IPlanSeeder planSeeder)
        {
            _settingSeeder = settingSeeder;
            _planSeeder = planSeeder;
        }

        public async Task InitializeAsync()
        {
            await _settingSeeder.SeedAsync();
            await _planSeeder.SeedAsync();
        }
    }
}
