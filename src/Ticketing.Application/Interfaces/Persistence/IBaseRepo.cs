using System.Linq.Expressions;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> GetAll();

    IQueryable<T> GetByCondition(Expression<Func<T, bool>> predicate);

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Delete(T entity);
}