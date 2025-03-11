using BusinessObjects.Models;
using Repositories.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.Implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly SPSSContext _context;
    private IProductRepository _productRepository;
    private ICancelReasonRepository _cancelReasonRepository;
    private IProductImageRepository _productImageRepository;
    private IAddressRepository _addressRepository;
    

    private IDbContextTransaction _transaction; 

    public UnitOfWork(SPSSContext context) =>  _context = context;
    
    public IProductImageRepository ProductImages => _productImageRepository ?? (_productImageRepository = new ProductImageRepository(_context));
    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public ICancelReasonRepository CancelReasons => _cancelReasonRepository ??= new CancelReasonRepository(_context);
    public IAddressRepository Addresses => _addressRepository ??= new AddressRepository(_context);

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