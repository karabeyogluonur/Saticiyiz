using System.Linq.Expressions;
using ST.Domain.Entities.Common;


namespace ST.Application.Interfaces.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll(bool tracking = false);

        Task<TEntity?> GetByIdAsync(int id);

        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression, bool tracking = false);

        Task AddAsync(TEntity entity);

        void Update(TEntity entity);

        void Remove(TEntity entity);
    }
}
