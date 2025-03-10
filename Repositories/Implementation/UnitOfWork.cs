using BusinessObjects.Models;
using Repositories.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.Implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly spss1Context _context;
    private IProductRepository _productRepository;
    private IDbContextTransaction _transaction; 

    public UnitOfWork(spss1Context context) =>  _context = context;

    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}