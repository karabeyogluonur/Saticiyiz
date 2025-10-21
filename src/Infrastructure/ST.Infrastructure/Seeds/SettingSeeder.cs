using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Seeds;
using ST.Application.Settings;
using ST.Domain.Entities.Configurations;
using ST.Domain.Interfaces;
using ST.Infrastructure.Persistence.Contexts;
using System.Reflection;

namespace ST.Infrastructure.Seeds
{
    public class SettingSeeder : ISettingSeeder, ISeeder
    {
        private readonly SharedDbContext _context;

        public SettingSeeder(SharedDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Seeder, tenant filtresinden etkilenmemelidir.
            // Sadece global (TenantId'si null olan) ayarları kontrol ediyoruz.
            var existingSettings = await _context.Settings
                .IgnoreQueryFilters()
                .Where(s => s.TenantId == null)
                .ToListAsync();

            var existingKeys = existingSettings.Select(s => s.Key).ToHashSet();

            var settingsToSeed = DiscoverAndMapSettings();
            var defaultKeys = settingsToSeed.Select(s => s.Key).ToHashSet();

            // Veritabanında eksik olan yeni global ayarları bul ve ekle
            var missingSettings = settingsToSeed
                .Where(s => !existingKeys.Contains(s.Key))
                .ToList();

            if (missingSettings.Any())
            {
                await _context.Settings.AddRangeAsync(missingSettings);
            }

            // Kodda artık var olmayan, eskimiş global ayarları bul ve sil
            var obsoleteSettings = existingSettings
                .Where(s => !defaultKeys.Contains(s.Key))
                .ToList();

            if (obsoleteSettings.Any())
            {
                _context.Settings.RemoveRange(obsoleteSettings);
            }

            // Değişiklikleri kaydet.
            await _context.SaveChangesAsync();
        }

        private List<Setting> DiscoverAndMapSettings()
        {
            // Bu metodun içeriğinde bir değişiklik yapmaya gerek yok.
            var applicationAssembly = typeof(SubscriptionSetting).Assembly;

            var settingTypes = applicationAssembly.GetTypes()
                .Where(t => typeof(ISetting).IsAssignableFrom(t)
                         && t.IsClass
                         && !t.IsAbstract
                         && t.GetConstructor(Type.EmptyTypes) != null)
                .ToList();

            var settingsToSeed = new List<Setting>();

            foreach (var settingType in settingTypes)
            {
                object defaultSettingInstance = Activator.CreateInstance(settingType);

                string prefix = (defaultSettingInstance is IGroupSetting groupSetting) ? groupSetting.GetPrefix() + "." : string.Empty;

                PropertyInfo[] properties = settingType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo prop in properties)
                {
                    string key = prefix + prop.Name;
                    object rawValue = prop.GetValue(defaultSettingInstance);
                    string value;

                    if (prop.PropertyType.IsEnum && rawValue != null)
                    {
                        value = Convert.ToInt32(rawValue).ToString();
                    }
                    else
                    {
                        value = rawValue?.ToString() ?? string.Empty;
                    }

                    string type = prop.PropertyType.Name;

                    settingsToSeed.Add(new Setting
                    {
                        Key = key,
                        Value = value,
                        Type = type,
                        TenantId = null, // Global ayar olduğunu belirtiyoruz.
                        CreatedBy = "System.Seeder",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            return settingsToSeed;
        }
    }
}