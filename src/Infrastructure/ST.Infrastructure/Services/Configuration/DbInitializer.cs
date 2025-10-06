using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Configuration;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Seeds;
using ST.Infrastructure.Tenancy;
using System;
using System.Threading.Tasks;

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