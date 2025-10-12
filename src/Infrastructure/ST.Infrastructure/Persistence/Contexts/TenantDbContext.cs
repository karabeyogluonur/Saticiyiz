using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Contexts;

namespace ST.Infrastructure.Persistence.Contexts
{
    public class TenantDbContext : DbContext, ITenantDbContext
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
