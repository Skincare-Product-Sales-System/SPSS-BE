using System.Linq.Expressions;

namespace Repositories.Interface;

public interface IRepositoryBase<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);

    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}