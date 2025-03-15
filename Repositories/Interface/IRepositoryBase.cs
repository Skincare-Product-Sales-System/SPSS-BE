using System.Linq.Expressions;

namespace Repositories.Interface;

public interface IRepositoryBase<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);
    IQueryable<T> Entities { get; }
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    void RemoveRange(IEnumerable<T> entities);
    void DetachEntities();
    IQueryable<T> GetQueryable();
    Task<IQueryable<T>> GetQueryableAsync();
}