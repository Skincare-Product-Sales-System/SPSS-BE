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
    private IAddressRepository _addressRepository;
    private IVariationRepository _variationRepository;
    private IVariationOptionRepository _variationOptionRepository;
    private IProductStatusRepository _productStatusRepository;
    private IProductCategoryRepository _productCategoryRepository;
    private IRefreshTokenRepository _refreshTokenRepository;
    private IBlogRepository _blogRepository;
    private IUserRepository _userRepository;
    private IRoleRepository _roleRepository;
    private IReviewRepository _reviewRepository;
    private IReplyRepository _replyRepository;
    private IPaymentMethodRepository _paymentMethodRepository;
    private ICartItemRepository _cartItemRepository;
    private IPromotionTypeRepository _promotionTypeRepository;
    private IBrandRepository _brandRepository;
    private IVoucherRepository _voucherRepository;
    private ISkinTypeRepository _skinTypeRepository;
    private IPromotionRepository _promotionRepository;
    private IDbContextTransaction _transaction; 
    private IOrderRepository _orderRepository;
    private IReviewImageRepository _reviewImageRepository;
    public UnitOfWork(SPSSContext context) =>  _context = context;
    
    public IProductImageRepository ProductImages => _productImageRepository ?? (_productImageRepository = new ProductImageRepository(_context));
    public IReviewImageRepository ReviewImages => _reviewImageRepository ?? (_reviewImageRepository = new ReviewImageRepository(_context));
    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
    public ISkinTypeRepository SkinTypes => _skinTypeRepository ??= new SkinTypeRepository(_context);
    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public ICancelReasonRepository CancelReasons => _cancelReasonRepository ??= new CancelReasonRepository(_context);
    public IProductConfigurationRepository ProductConfigurations => _productConfigurationRepository ??= new ProductConfigurationRepository(_context);
    public IProductItemRepository ProductItems => _productItemRepository ??= new ProductItemRepository(_context);
    public IVariationRepository Variations => _variationRepository ??= new VariationRepository(_context);
    public IBrandRepository Brands => _brandRepository ??= new BrandRepository(_context);
    public IVariationOptionRepository VariationOptions => _variationOptionRepository ??= new VariationOptionRepository(_context);
    public IProductStatusRepository ProductStatuses => _productStatusRepository ??= new ProductStatusRepository(_context);
    public IProductCategoryRepository ProductCategories => _productCategoryRepository ??= new ProductCategoryRepository(_context);
    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IAddressRepository Addresses => _addressRepository ??= new AddressRepository(_context);
    public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);
    public IBlogRepository Blogs => _blogRepository ??= new BlogRepository(_context);
    public IReviewRepository Reviews => _reviewRepository ??= new ReviewRepository(_context);
    public IReplyRepository Replies => _replyRepository ??= new ReplyRepository(_context);
    public IPromotionTypeRepository PromotionTypes => _promotionTypeRepository ??= new PromotionTypeRepository(_context);
    public IPaymentMethodRepository PaymentMethods => _paymentMethodRepository ??= new PaymentMethodRepository(_context);
    public ICartItemRepository CartItems => _cartItemRepository ??= new CartItemRepository(_context);
    public IVoucherRepository Vouchers => _voucherRepository ??= new VoucherRepository(_context);
    public IPromotionRepository Promotions => _promotionRepository ??= new PromotionRepository(_context);
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