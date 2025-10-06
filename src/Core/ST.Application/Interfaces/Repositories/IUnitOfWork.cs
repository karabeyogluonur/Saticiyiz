using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ST.Application.Interfaces.Repositories;



public interface IUnitOfWork : IDisposable
{
    void ChangeDatabase(string database);

    IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class;

    int SaveChanges(bool ensureAutoHistory = false);

    Task<int> SaveChangesAsync(bool ensureAutoHistory = false);

    int ExecuteSqlCommand(string sql, params object[] parameters);

    IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;

    void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback);
}
