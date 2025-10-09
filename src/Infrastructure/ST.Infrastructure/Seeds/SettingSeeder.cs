using ST.Application.Settings;
using ST.Infrastructure.Persistence.Contexts;
using System.Reflection;
using ST.Domain.Entities.Configurations;
using ST.Domain.Interfaces;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Seeds;

namespace ST.Infrastructure.Seeds
{
    public class SettingSeeder : ISettingSeeder, ISeeder
    {
        private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
        private readonly IRepository<Setting> _settingRepository;

        public SettingSeeder(IUnitOfWork<ApplicationDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _settingRepository = _unitOfWork.GetRepository<Setting>();
        }

        public async Task SeedAsync()
        {
            IList<Setting> existingSettings = await _settingRepository.GetAllAsync();
            HashSet<string> existingKeys = existingSettings.Select(s => s.Key).ToHashSet();

            List<Setting> settingsToSeed = DiscoverAndMapSettings();
            HashSet<string> defaultKeys = settingsToSeed.Select(s => s.Key).ToHashSet();

            List<Setting> missingSettings = settingsToSeed
                .Where(s => !existingKeys.Contains(s.Key))
                .ToList();

            if (missingSettings.Any())
            {
                await _settingRepository.InsertAsync(missingSettings);
            }

            List<Setting> obsoleteSettings = existingSettings
                .Where(s => !defaultKeys.Contains(s.Key))
                .ToList();

            if (obsoleteSettings.Any())
            {
                _settingRepository.Delete(obsoleteSettings);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private List<Setting> DiscoverAndMapSettings()
        {
            Assembly applicationAssembly = typeof(SubscriptionSetting).Assembly;

            List<Type> settingTypes = applicationAssembly.GetTypes()
                .Where(t => typeof(ISetting).IsAssignableFrom(t)
                         && t.IsClass
                         && !t.IsAbstract
                         && t.GetConstructor(Type.EmptyTypes) != null)
                .ToList();

            List<Setting> settingsToSeed = new List<Setting>();

            foreach (Type settingType in settingTypes)
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