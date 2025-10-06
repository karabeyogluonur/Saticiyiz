using ST.Application.Settings; // SubscriptionSetting'in bulunduğu Assembly'yi almak için
using ST.Infrastructure.Persistence.Contexts; // ApplicationDbContext'in bulunduğu yer
using System.Reflection;
using ST.Domain.Entities.Configurations;
using ST.Domain.Interfaces;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Seeds;

namespace ST.Infrastructure.Seeds
{
    // ISeeder, DbInitializer tarafından çağrılacak temel arayüzdür.
    public class SettingSeeder : ISettingSeeder, ISeeder
    {
        // Generic UoW ve Repository kullanımı
        private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
        private readonly IRepository<Setting> _settingRepository;

        public SettingSeeder(IUnitOfWork<ApplicationDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            // Repository'yi UoW üzerinden al
            _settingRepository = _unitOfWork.GetRepository<Setting>();
        }

        public async Task SeedAsync()
        {
            // Veritabanında herhangi bir ayar kaydı varsa, tekrar yüklemeyi atla.
            if (await _settingRepository.ExistsAsync())
            {
                return;
            }

            // Dinamik olarak keşfedilen ayar listesini al
            var settingsToSeed = DiscoverAndMapSettings();

            // Tüm ayarları veritabanına toplu olarak kaydet
            await _settingRepository.InsertAsync(settingsToSeed);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Application Assembly'sini tarar, ISetting uygulayan sınıfları bulur ve onları Setting Entity'lerine eşler.
        /// </summary>
        private List<Setting> DiscoverAndMapSettings()
        {
            // Hedef Assembly'yi Belirleme (SubscriptionSetting'in bulunduğu Application katmanı)
            var applicationAssembly = typeof(SubscriptionSetting).Assembly;

            // ISetting uygulayan, soyut olmayan ve varsayılan kurucusu olan tipleri bul
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

                // Prefix Kontrolü: IGroupSetting'den prefix'i al (Örn: "Subscription.")
                string prefix = string.Empty;
                if (defaultSettingInstance is IGroupSetting groupSetting)
                {
                    prefix = groupSetting.GetPrefix() + ".";
                }

                var properties = settingType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    // Mapping işlemi
                    string key = prefix + prop.Name;
                    string value = prop.GetValue(defaultSettingInstance)?.ToString() ?? string.Empty;
                    string type = prop.PropertyType.Name;

                    settingsToSeed.Add(new Setting
                    {
                        Key = key,
                        Value = value,
                        Type = type,
                        TenantId = null, // GLOBAL ayar olduğu için NULL (Shared DB kuralı)

                        // Denetim Alanları
                        CreatedBy = "System.Seeder",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            return settingsToSeed;
        }
    }
}