using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Seeds;
using ST.Application.Settings;
using ST.Domain.Entities.Configurations;
using ST.Domain.Interfaces;
using System.Reflection;

namespace ST.Infrastructure.Seeds
{
    public class SettingSeeder : ISettingSeeder, ISeeder
    {
        private readonly IUnitOfWork _unitOfWork;

        public SettingSeeder(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SeedAsync()
        {

            var existingSettings = await _unitOfWork.Settings.GetAll()
                .IgnoreQueryFilters()
                .Where(s => s.TenantId == null)
                .ToListAsync();

            var existingKeys = existingSettings.Select(s => s.Key).ToHashSet();

            var settingsToSeed = DiscoverAndMapSettings();
            var defaultKeys = settingsToSeed.Select(s => s.Key).ToHashSet();

            var missingSettings = settingsToSeed
                .Where(s => !existingKeys.Contains(s.Key))
                .ToList();

            if (missingSettings.Any())
            {
                foreach (var setting in missingSettings)
                {
                    await _unitOfWork.Settings.AddAsync(setting);
                }
            }

            var obsoleteSettings = existingSettings
                .Where(s => !defaultKeys.Contains(s.Key))
                .ToList();

            if (obsoleteSettings.Any())
            {
                foreach (var setting in obsoleteSettings)
                {
                    _unitOfWork.Settings.Remove(setting);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private List<Setting> DiscoverAndMapSettings()
        {
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

                string prefix = string.Empty;
                if (defaultSettingInstance is IGroupSetting groupSetting)
                {
                    prefix = groupSetting.GetPrefix() + ".";
                }

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
                        TenantId = null,
                        CreatedBy = "System.Seeder",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            return settingsToSeed;
        }
    }
}
