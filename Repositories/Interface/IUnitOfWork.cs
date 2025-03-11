namespace Repositories.Interface;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IProductImageRepository ProductImages { get; }
    ICancelReasonRepository CancelReasons { get; }
    IProductItemRepository ProductItems { get; }
    IProductConfigurationRepository ProductConfigurations { get; }
    IVariationRepository Variations { get; }
    IVariationOptionRepository VariationOptions { get; }
    IProductStatusRepository ProductStatuses { get; }
    IProductCategoryRepository  ProductCategories { get; }
    IReviewRepository Reviews { get; }
    IReplyRepository Replies { get; }

    ICartItemRepository CartItems { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}