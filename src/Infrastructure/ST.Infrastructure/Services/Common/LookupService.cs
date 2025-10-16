using Microsoft.EntityFrameworkCore;
using ST.Application.DTOs.Common;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities.Lookup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST.Infrastructure.Services.Common
{
    public class LookupService : ILookupService
    {
        // City ve District SharedDbContext'te ise ISharedDbContext'ten gelen repository'ler kullanılmalı.
        // Proje yapınıza göre doğru repository'yi inject edin.
        private readonly IUnitOfWork _unitOfWork;

        public LookupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<LookupDto>> GetCitiesAsync()
        {
            return await _unitOfWork.Cities.GetAll()
                .Select(c => new LookupDto { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<LookupDto>> GetDistrictsByCityAsync(int cityId)
        {
            return await _unitOfWork.Districts.GetAll()
                .Where(d => d.CityId == cityId)
                .Select(d => new LookupDto { Id = d.Id, Name = d.Name })
                .ToListAsync();
        }
    }
}