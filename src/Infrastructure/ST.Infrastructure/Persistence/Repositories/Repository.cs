using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ST.Infrastructure.Persistence.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(DbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> GetAll(bool tracking = false)
        {
            return tracking ? DbSet.AsQueryable() : DbSet.AsNoTracking();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression, bool tracking = false)
        {
            return await GetAll(tracking).FirstOrDefaultAsync(expression);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }
    }
}
