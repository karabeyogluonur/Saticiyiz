

using Microsoft.EntityFrameworkCore;

namespace ST.Application.Interfaces.Repositories;

public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    TContext DbContext { get; }

    Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks);
}
