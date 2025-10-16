using ST.Application.DTOs.Common;

namespace ST.Application.Interfaces.Common;

public interface ILookupService
{
    Task<IEnumerable<LookupDto>> GetCitiesAsync();
    Task<IEnumerable<LookupDto>> GetDistrictsByCityAsync(int cityId);
}