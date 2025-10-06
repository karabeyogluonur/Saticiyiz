using Microsoft.AspNetCore.Identity;

namespace ST.Application.Interfaces.Identity
{
    public interface IUserService
    {
        /// <summary>
        /// Yeni bir kullanıcı oluşturur ve kullanıcıyı belirtilen kiracıya (tenant) bağlar.
        /// Bu, Multi-Tenancy'de kritik bir işlemdir.
        /// </summary>
        /// <param name="firstName">Kullanıcının adı.</param>
        /// <param name="lastName">Kullanıcının soyadı.</param>
        /// <param name="email">Kullanıcının e-posta adresi (aynı zamanda kullanıcı adı).</param>
        /// <param name="password">Kullanıcının şifresi.</param>
        /// <param name="tenantId">Kullanıcının ait olduğu kiracının ID'si (Zorunlu).</param>
        /// <returns>Identity sonuçları ve oluşturulan kullanıcının ID'si (Tuple).</returns>
        Task<(IdentityResult Result, string UserId)> CreateUserWithTenantAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string tenantId);

        /// <summary>
        /// Belirtilen kullanıcıya bir rolü atar.
        /// </summary>
        /// <param name="userId">Kullanıcının ID'si.</param>
        /// <param name="roleName">Atanacak rolün adı (Örn: AppRoles.Admin).</param>
        Task AddUserToTenantRoleAsync(string userId, string roleName);

        /// <summary>
        /// Kullanıcının kiracı ID'sini döndürür. Kiracı bazlı yetkilendirme için kullanılır.
        /// </summary>
        /// <param name="userId">Kullanıcının ID'si.</param>
        Task<string?> GetTenantIdByUserIdAsync(string userId);

        /// <summary>
        /// Bir kullanıcının sisteme giriş bilgilerini doğrular (SignInManager'ın işlevini soyutlar).
        /// </summary>
        /// <param name="email">Kullanıcı e-posta adresi.</param>
        /// <param name="password">Şifre.</param>
        /// <returns>Giriş başarılı ise kullanıcı ID'si, değilse null.</returns>
        Task<string?> CheckPasswordSignInAsync(string email, string password);

        /// <summary>
        /// (Gelecekteki erişim kontrolü için) Kullanıcının rollerini döndürür.
        /// </summary>
        /// <param name="userId">Kullanıcının ID'si.</param>
        Task<IList<string>> GetRolesAsync(string userId);
    }
}