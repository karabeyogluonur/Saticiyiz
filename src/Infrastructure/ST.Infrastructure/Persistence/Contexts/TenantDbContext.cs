using Microsoft.EntityFrameworkCore;

namespace ST.Infrastructure.Persistence.Contexts
{
    public class TenantDbContext : DbContext
    {

        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
