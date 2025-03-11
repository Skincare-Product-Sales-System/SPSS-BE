using BusinessObjects.Models;
using Repositories.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly SPSSContext _context;
    private IProductRepository _productRepository;
    private ICancelReasonRepository _cancelReasonRepository;
    private IProductImageRepository _productImageRepository;
    private IProductConfigurationRepository _productConfigurationRepository;
    private IProductItemRepository _productItemRepository;
    private IVariationRepository _variationRepository;
    private IVariationOptionRepository _variationOptionRepository;
    private IProductStatusRepository _productStatusRepository;
    private IProductCategoryRepository _productCategoryRepository;
    private IAddressRepository _addressRepository;
    

    private IDbContextTransaction _transaction; 
    private IDbContextTransaction _transaction;
    private IReviewRepository _reviewRepository;
    private IReplyRepository _replyRepository;
    private ICartItemRepository _cartItemRepository;

    public UnitOfWork(SPSSContext context) =>  _context = context;
    
    public IProductImageRepository ProductImages => _productImageRepository ?? (_productImageRepository = new ProductImageRepository(_context));
    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public ICancelReasonRepository CancelReasons => _cancelReasonRepository ??= new CancelReasonRepository(_context);
    public IProductConfigurationRepository ProductConfigurations => _productConfigurationRepository ??= new ProductConfigurationRepository(_context);
    public IProductItemRepository ProductItems => _productItemRepository ??= new ProductItemRepository(_context);
    public IVariationRepository Variations => _variationRepository ??= new VariationRepository(_context);
    public IVariationOptionRepository VariationOptions => _variationOptionRepository ??= new VariationOptionRepository(_context);
    public IProductStatusRepository ProductStatuses => _productStatusRepository ??= new ProductStatusRepository(_context);
    public IProductCategoryRepository ProductCategories => _productCategoryRepository ??= new ProductCategoryRepository(_context);
    public IAddressRepository Addresses => _addressRepository ??= new AddressRepository(_context);
    public IReviewRepository Reviews => _reviewRepository ??= new ReviewRepository(_context);
    public IReplyRepository Replies => _replyRepository ??= new ReplyRepository(_context);

    public ICartItemRepository CartItems => _cartItemRepository ??= new CartItemRepository(_context);

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