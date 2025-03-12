namespace Repositories.Interface;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IProductImageRepository ProductImages { get; }
    ICancelReasonRepository CancelReasons { get; }
    IProductItemRepository ProductItems { get; }
    IAddressRepository Addresses { get; }
    IProductConfigurationRepository ProductConfigurations { get; }
    IVariationRepository Variations { get; }
    IVariationOptionRepository VariationOptions { get; }
    IProductStatusRepository ProductStatuses { get; }
    IProductCategoryRepository  ProductCategories { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IBlogRepository Blogs { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}