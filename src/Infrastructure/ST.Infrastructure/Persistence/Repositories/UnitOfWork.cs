using Finbuckle.MultiTenant.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Events.Common;

namespace ST.Infrastructure.Persistence.Repositories;

public class UnitOfWork<TContext> : IRepositoryFactory, IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private bool disposed = false;
    private Dictionary<Type, object> repositories;
    private IDbContextTransaction _transaction;

    public UnitOfWork(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public TContext DbContext => _context;

    public void ChangeDatabase(string database)
    {
        System.Data.Common.DbConnection connection = _context.Database.GetDbConnection();
        if (connection.State.HasFlag(ConnectionState.Open))
        {
            connection.ChangeDatabase(database);
        }
        else
        {
            string connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
            connection.ConnectionString = connectionString;
        }

        IEnumerable<IEntityType> items = _context.Model.GetEntityTypes();
        foreach (IEntityType item in items)
        {
            if (item is IConventionEntityType entityType)
            {
                entityType.SetSchema(database);
            }
        }
    }

    public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class
    {
        if (repositories == null)
        {
            repositories = new Dictionary<Type, object>();
        }

        if (hasCustomRepository)
        {
            IRepository<TEntity> customRepo = _context.GetService<IRepository<TEntity>>();
            if (customRepo != null)
            {
                return customRepo;
            }
        }

        Type type = typeof(TEntity);
        if (!repositories.ContainsKey(type))
        {
            repositories[type] = new Repository<TEntity>(_context);
        }

        return (IRepository<TEntity>)repositories[type];
    }

    public int ExecuteSqlCommand(string sql, params object[] parameters) => _context.Database.ExecuteSqlRaw(sql, parameters);

    public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class => _context.Set<TEntity>().FromSqlRaw(sql, parameters);

    public int SaveChanges(bool ensureAutoHistory = false)
    {
        if (ensureAutoHistory)
        {
            _context.EnsureAutoHistory();
        }

        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false)
    {
        if (ensureAutoHistory)
        {
            _context.EnsureAutoHistory();
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks)
    {
        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            int count = 0;
            foreach (IUnitOfWork unitOfWork in unitOfWorks)
            {
                count += await unitOfWork.SaveChangesAsync(ensureAutoHistory).ConfigureAwait(false);
            }

            count += await SaveChangesAsync(ensureAutoHistory);

            ts.Complete();

            return count;
        }
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                if (repositories != null)
                {
                    repositories.Clear();
                }

                _context.Dispose();
            }
        }

        disposed = true;
    }

    public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
    {
        _context.ChangeTracker.TrackGraph(rootEntity, callback);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    public IEnumerable<DomainEvent> GetDomainEvents()
    {
        return _context.ChangeTracker.Entries<IDomainEvent>()
            .Select(e => e.Entity)
            .SelectMany(e => e.DomainEvents)
            .ToList();
    }

    public void ClearDomainEvents()
    {
        List<IDomainEvent> domainEventEntities = _context.ChangeTracker.Entries<IDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        domainEventEntities.ForEach(e => e.ClearDomainEvents());
    }
}