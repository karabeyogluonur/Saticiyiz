namespace ST.Application.Interfaces.Tenancy
{
    /// <summary>
    /// O anki isteğin tenant'ına göre doğru veritabanı bağlantı dizesini çözer.
    /// </summary>
    public interface ITenantResolver
    {
        /// <summary>
        /// Tenant'a özel veritabanı için bağlantı dizesini döndürür.
        /// Eğer tenant'ın özel bir veritabanı yoksa, ana (shared) veritabanının bağlantı dizesini döndürür.
        /// </summary>
        Task<string> GetTenantConnectionStringAsync();
    }
}