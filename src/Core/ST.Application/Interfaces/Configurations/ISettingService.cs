using ST.Domain.Interfaces;

namespace ST.Application.Interfaces.Configuration
{
    public interface ISettingService
    {
        public interface ISettingService
        {
            /// <summary>
            /// Global ayar tablosundaki (Setting) anahtarlara karşılık gelen değerleri çeker ve 
            /// tümünü tek bir Strong-Typed nesneye (TSetting) map eder.
            /// </summary>
            /// <typeparam name="TSetting">ISetting arayüzünü uygulayan hedef nesne.</typeparam>
            Task<TSetting> GetGlobalSettingsAsync<TSetting>()
                where TSetting : ISetting, new();
        }
    }
}