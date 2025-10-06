using ST.Domain.Interfaces;

namespace ST.Application.Interfaces.Configuration
{
    public interface ISettingService
    {
        Task<TSetting> GetGlobalSettingsAsync<TSetting>()
            where TSetting : ISetting, new();
    }
}