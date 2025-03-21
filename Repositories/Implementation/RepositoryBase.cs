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

    public async Task<IQueryable<T>> GetQueryableAsync()
    {
        return await Task.FromResult(_context.Set<T>().AsQueryable());
    }

    public void Attach(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Set<T>().Attach(entity);
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

        // Kiểm tra nếu thuộc tính "CreatedTime" tồn tại
        var entityType = _context.Model.FindEntityType(typeof(T));
        bool hasCreatedTime = entityType?.FindProperty("CreatedTime") != null;

        if (hasCreatedTime)
        {
            query = query.OrderBy(e => EF.Property<DateTimeOffset>(e, "CreatedTime"));
        }
        else
        {
            // Nếu không có "CreatedTime", sắp xếp theo khóa chính hoặc mặc định
            var primaryKey = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();
            if (primaryKey != null)
            {
                query = query.OrderBy(e => EF.Property<object>(e, primaryKey.Name));
            }
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        var query = _context.Set<T>().AsQueryable();

        // Áp dụng bộ lọc nếu có
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Trả về danh sách tất cả các đối tượng
        return await query.ToListAsync();
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

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null.");

        return await _context.Set<T>().SingleOrDefaultAsync(predicate);
    }

    public void Add(T entity) => _context.Set<T>().Add(entity);
    public void AddRange(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities), "Entities to add cannot be null or empty.");

        _context.Set<T>().AddRange(entities);
    }

    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
    public void RemoveRange(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities), "Entities to delete cannot be null or empty.");

        _context.Set<T>().RemoveRange(entities);
    }

}