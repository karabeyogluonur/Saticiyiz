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
            if (await _settingRepository.ExistsAsync())
            {
                return;
            }

            var settingsToSeed = DiscoverAndMapSettings();

            await _settingRepository.InsertAsync(settingsToSeed);
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
                var defaultSettingInstance = Activator.CreateInstance(settingType);

                string prefix = string.Empty;
                if (defaultSettingInstance is IGroupSetting groupSetting)
                {
                    prefix = groupSetting.GetPrefix() + ".";
                }

                var properties = settingType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    string key = prefix + prop.Name;
                    string value = prop.GetValue(defaultSettingInstance)?.ToString() ?? string.Empty;
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