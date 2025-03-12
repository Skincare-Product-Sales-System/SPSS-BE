namespace Repositories.Interface;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IProductImageRepository ProductImages { get; }
    ICancelReasonRepository CancelReasons { get; }
    IProductItemRepository ProductItems { get; }
    IProductConfigurationRepository ProductConfigurations { get; }
    IBrandRepository Brands { get; }
    IVariationRepository Variations { get; }
    IVariationOptionRepository VariationOptions { get; }
    IProductStatusRepository ProductStatuses { get; }
    IProductCategoryRepository  ProductCategories { get; }
    IAddressRepository Addresses { get; }
    IReviewRepository Reviews { get; }
    IReplyRepository Replies { get; }
    IPromotionTypeRepository PromotionTypes { get; }
    ICartItemRepository CartItems { get; }
    IPaymentMethodRepository PaymentMethods { get; }
    ISkinTypeRepository SkinTypes { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}