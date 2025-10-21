using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Contexts;
using ST.Application.Interfaces.Seeds;
using ST.Domain.Entities.Subscriptions;

namespace ST.Infrastructure.Seeds
{
    public class PlanSeeder : IPlanSeeder
    {
        private readonly ISharedDbContext _context;

        public PlanSeeder(ISharedDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // 1. Deneme (Trial) Planını Kontrol Et ve Oluştur
            bool hasTrialPlan = await _context.Plans.AnyAsync(p => p.IsTrial);
            if (!hasTrialPlan)
            {
                var trialPlan = new Plan
                {
                    Name = "Deneme Planı",
                    Description = "14 günlük ücretsiz deneme.",
                    Price = 0,
                    IsActive = true,
                    IsTrial = true,
                    IsDefault = false, // Deneme planı, varsayılan plan olmamalıdır.
                    CreatedBy = "System.Seeder",
                    CreatedDate = DateTime.UtcNow
                };
                await _context.Plans.AddAsync(trialPlan);
            }

            // 2. Varsayılan (Default) Planı Kontrol Et ve Oluştur
            bool hasDefaultPlan = await _context.Plans.AnyAsync(p => p.IsDefault);
            if (!hasDefaultPlan)
            {
                var defaultPlan = new Plan
                {
                    Name = "Temel Plan",
                    Description = "Aboneliği biten veya yeni başlayanlar için temel özellikler.",
                    Price = 0,
                    IsActive = true,
                    IsTrial = false,
                    IsDefault = true, // Bu plan, varsayılan atama planıdır.
                    CreatedBy = "System.Seeder",
                    CreatedDate = DateTime.UtcNow
                };
                await _context.Plans.AddAsync(defaultPlan);
            }

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
