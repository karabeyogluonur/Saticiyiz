using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Seeds;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Interfaces;
using ST.Infrastructure.Persistence.Contexts; // DbContext için using

namespace ST.Infrastructure.Seeds
{
    public class PlanSeeder : IPlanSeeder, ISeeder
    {
        // IUnitOfWork yerine doğrudan SharedDbContext'e bağımlı oluyoruz.
        private readonly SharedDbContext _context;

        public PlanSeeder(SharedDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Plan tablosu global olduğu için tenant filtresi uygulanmaz.
            // Bu nedenle IgnoreQueryFilters() demeye gerek yoktur.
            bool existDefaultPlan = await _context.Plans
                .AnyAsync(plan => plan.IsDefault == true && plan.IsActive == true);

            if (existDefaultPlan)
            {
                return; // Varsayılan plan zaten varsa, hiçbir şey yapma.
            }

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

            await _context.Plans.AddAsync(plan);
            await _context.SaveChangesAsync();
        }
    }
}