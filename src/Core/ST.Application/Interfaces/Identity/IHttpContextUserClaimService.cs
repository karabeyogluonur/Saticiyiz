using System.Threading.Tasks;

namespace ST.Application.Interfaces.Identity
{
    public interface IHttpContextUserClaimService
    {
        /// <summary>
        /// Claim ekler veya var ise günceller.
        /// </summary>
        Task AddOrUpdateClaimAsync(string claimType, string claimValue);

        /// <summary>
        /// Claim siler.
        /// </summary>
        Task RemoveClaimAsync(string claimType);

        /// <summary>
        /// Claim değerini döner.
        /// </summary>
        Task<string?> GetClaimValueAsync(string claimType);
    }
}
