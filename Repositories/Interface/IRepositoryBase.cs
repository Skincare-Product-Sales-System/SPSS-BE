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
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    void DetachEntities();
    IQueryable<T> GetQueryable();
}