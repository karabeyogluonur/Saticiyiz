using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Settings;
using ST.Domain.Entities.Configurations;
using ST.Domain.Interfaces;
using System.Globalization;
using System.Reflection;

namespace ST.Infrastructure.Services.Configuration
{
    public class SettingService : ISettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantStore _currentTenantStore;
        private readonly IMemoryCache _cache;

        public SettingService(IUnitOfWork unitOfWork, ICurrentTenantStore currentTenantStore, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _currentTenantStore = currentTenantStore;
            _cache = cache;
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            var tenantId = _currentTenantStore.Id;
            var cacheKey = $"setting:{tenantId?.ToString() ?? "global"}:{key}";

            if (_cache.TryGetValue(cacheKey, out T cachedValue))
            {
                return cachedValue;
            }

            var setting = await FindSettingByKeyAsync(key);

            if (setting == null)
            {
                return default;
            }

            var value = ConvertValue<T>(setting.Value, typeof(T));

            _cache.Set(cacheKey, value, TimeSpan.FromHours(1));

            return value;
        }

        public async Task UpdateValueAsync<T>(string key, T value)
        {
            if (_currentTenantStore.Id == null)
                throw new InvalidOperationException("Global settings cannot be updated from this service. Use seeder for global settings.");

            var tenantId = _currentTenantStore.Id.Value;
            var settingRepository = _unitOfWork.Settings;

            var setting = await settingRepository.GetAsync(s => s.Key == key && s.TenantId == tenantId);

            string valueToSave = value?.ToString() ?? string.Empty;

            if (setting != null)
            {
                setting.Value = valueToSave;
                settingRepository.Update(setting);
            }
            else
            {
                var newSetting = new Setting
                {
                    Key = key,
                    Value = valueToSave,
                    TenantId = tenantId,
                    Type = typeof(T).Name,
                    CreatedBy = "System.SettingService",
                    CreatedDate = DateTime.UtcNow
                };
                await settingRepository.AddAsync(newSetting);
            }

            await _unitOfWork.SaveChangesAsync();

            var cacheKey = $"setting:{tenantId}:{key}";
            _cache.Remove(cacheKey);
        }

        public async Task<TSetting> GetGlobalSettingsAsync<TSetting>()
            where TSetting : ISetting, new()
        {
            var result = new TSetting();
            string prefix = (result is IGroupSetting group) ? group.GetPrefix() + "." : string.Empty;

            var properties = typeof(TSetting).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var keys = properties.Select(p => prefix + p.Name).ToList();

            var settingsFromDb = await _unitOfWork.Settings
                .GetAll()
                .IgnoreQueryFilters()
                .Where(s => keys.Contains(s.Key) && s.TenantId == null)
                .ToListAsync();

            if (!settingsFromDb.Any() && keys.Any())
                throw new NotFoundException($"Expected Global Settings ({typeof(TSetting).Name}) were not found in the database.");

            foreach (var prop in properties)
            {
                string key = prefix + prop.Name;
                var dbSetting = settingsFromDb.FirstOrDefault(s => s.Key == key);

                if (dbSetting != null && prop.CanWrite)
                {
                    var convertedValue = ConvertValue(dbSetting.Value, prop.PropertyType);
                    prop.SetValue(result, convertedValue);
                }
            }

            return result;
        }

        private async Task<Setting> FindSettingByKeyAsync(string key)
        {
            var settingRepository = _unitOfWork.Settings;
            var tenantId = _currentTenantStore.Id;

            return await settingRepository.GetAll()
                .OrderByDescending(s => s.TenantId)
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        private T ConvertValue<T>(string value, Type targetType)
        {
            try
            {
                if (targetType.IsEnum)
                {
                    int enumValue = int.Parse(value, CultureInfo.InvariantCulture);
                    return (T)Enum.ToObject(targetType, enumValue);
                }
                return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Setting value '{value}' could not be converted to target type {targetType.Name}.", ex);
            }
        }

        private object ConvertValue(string value, Type targetType)
        {
            try
            {
                if (targetType.IsEnum)
                {
                    int enumValue = int.Parse(value, CultureInfo.InvariantCulture);
                    return Enum.ToObject(targetType, enumValue);
                }

                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Setting value '{value}' could not be converted to target type {targetType.Name}.", ex);
            }
        }

    }
}
