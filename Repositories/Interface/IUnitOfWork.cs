namespace Repositories.Interface;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IProductImageRepository ProductImages { get; }
    ICancelReasonRepository CancelReasons { get; }
    IProductItemRepository ProductItems { get; }
    IAddressRepository Addresses { get; }
    IProductConfigurationRepository ProductConfigurations { get; }
    IBrandRepository Brands { get; }
    IVariationRepository Variations { get; }
    IVariationOptionRepository VariationOptions { get; }
    IProductStatusRepository ProductStatuses { get; }
    IProductCategoryRepository  ProductCategories { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IBlogRepository Blogs { get; }
    IReviewRepository Reviews { get; }
    IReplyRepository Replies { get; }
    IPromotionTypeRepository PromotionTypes { get; }
    ICartItemRepository CartItems { get; }
    IPaymentMethodRepository PaymentMethods { get; }
    IVoucherRepository Vouchers { get; }
    ISkinTypeRepository SkinTypes { get; }
    IPromotionRepository Promotions { get; }
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}