using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Seeds;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Interfaces;

namespace ST.Infrastructure.Seeds
{
    public class PlanSeeder : IPlanSeeder, ISeeder
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlanSeeder(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SeedAsync()
        {
            bool existDefaultPlan = await _unitOfWork.Plans.GetAll()
                .AnyAsync(plan => plan.IsDefault == true && plan.IsActive == true);

            if (existDefaultPlan)
                return;

            var plan = new Plan
            {
                Name = "Trial",
                Description = "Kayıt olan üyeler için standart deneme süresi",
                IsActive = true,
                IsDefault = true,
                CreatedBy = "System.Seeder",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,
                Price = 0,
            };

            await _unitOfWork.Plans.AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
