using System.Linq.Expressions;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System.Linq.Expressions;

namespace Repositories.Implementation;

public class RepositoryBase<T, TKey> : IRepositoryBase<T, TKey> where T : class
{
    protected readonly SPSSContext _context;

    public RepositoryBase(SPSSContext context) => _context = context;
    public IQueryable<T> GetQueryable()
    {
        return _context.Set<T>().AsQueryable();
    }
    public async Task<T?> GetByIdAsync(TKey id) => await _context.Set<T>().FindAsync(id);
    public IQueryable<T> Entities => _context.Set<T>();
    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>> predicate)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        var query = _context.Set<T>().AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        int totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => EF.Property<DateTimeOffset>(e, "CreatedTime"))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    public void DetachEntities()
    {
        var entries = _context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
            }
        }
    }

    public void Add(T entity) => _context.Set<T>().Add(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
}