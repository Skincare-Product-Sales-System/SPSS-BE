using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation;

public class RepositoryBase<T, TKey> : IRepositoryBase<T, TKey> where T : class
{
    protected readonly SPSSContext _context;

    public RepositoryBase(SPSSContext context) => _context = context;

    public async Task<T?> GetByIdAsync(TKey id) => await _context.Set<T>().FindAsync(id);
    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        var query = _context.Set<T>().AsQueryable();
        int totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
    public void Add(T entity) => _context.Set<T>().Add(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
}