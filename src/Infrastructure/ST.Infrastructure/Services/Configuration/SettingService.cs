using ST.Application.Interfaces.Configuration;
using System.Reflection;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities.Configurations;
using ST.Application.Exceptions;
using ST.Infrastructure.Persistence.Contexts;
using ST.Domain.Interfaces;

namespace ST.Infrastructure.Services.Configuration
{
    public class SettingService : ISettingService
    {
        private readonly IUnitOfWork<ApplicationDbContext> _applicationUnitOfWork;
        private readonly IRepository<Setting> _settingRepository;

        public SettingService(IUnitOfWork<ApplicationDbContext> applicationUnitOfWork)
        {
            _applicationUnitOfWork = applicationUnitOfWork;
            _settingRepository = _applicationUnitOfWork.GetRepository<Setting>();
        }

        public async Task<TSetting> GetGlobalSettingsAsync<TSetting>()
            where TSetting : ISetting, new()
        {
            PropertyInfo[] properties = typeof(TSetting).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> keys = properties.Select(p => p.Name).ToList();

            List<Setting> settingsFromDb = await _settingRepository
                .GetAll()
                .Where(s => keys.Contains(s.Key) && s.TenantId == null)
                .ToListAsync();

            if (!settingsFromDb.Any() && keys.Any())
            {
                throw new NotFoundException($"Expected Global Settings ({typeof(TSetting).Name}) were not found in the database.");
            }

            TSetting result = new TSetting();

            foreach (PropertyInfo prop in properties)
            {
                Setting dbSetting = settingsFromDb.FirstOrDefault(s => s.Key == prop.Name);

                if (dbSetting != null && prop.CanWrite)
                {
                    try
                    {
                        object convertedValue = Convert.ChangeType(dbSetting.Value, prop.PropertyType, CultureInfo.InvariantCulture);
                        prop.SetValue(result, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Setting key '{prop.Name}' ('{dbSetting.Value}') could not be converted to target type {prop.PropertyType.Name}.", ex);
                    }
                }
            }

            return result;
        }
    }
}