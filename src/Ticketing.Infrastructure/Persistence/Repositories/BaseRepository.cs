using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Infrastructure.Persistence.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<T> GetAll()
    {
        return _context.Set<T>().AsQueryable();
    }

    public IQueryable<T> GetByCondition(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().Where(predicate);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}