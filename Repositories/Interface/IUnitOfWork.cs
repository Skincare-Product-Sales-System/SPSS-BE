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
    IBlogSectionRepository BlogSections { get; }
    IVariationOptionRepository VariationOptions { get; }
    ISkinTypeRoutineStepRepository SkinTypeRoutineSteps { get; }
    IProductStatusRepository ProductStatuses { get; }
    IProductCategoryRepository  ProductCategories { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IBlogRepository Blogs { get; }
    IReviewRepository Reviews { get; }
    IReplyRepository Replies { get; }
    ICartItemRepository CartItems { get; }
    IPaymentMethodRepository PaymentMethods { get; }
    IVoucherRepository Vouchers { get; }
    ISkinTypeRepository SkinTypes { get; }
    IOrderRepository Orders { get; }
    IReviewImageRepository ReviewImages { get; }
    IOrderDetailRepository OrderDetails { get; }
    IStatusChangeRepository StatusChanges { get; }
    IProductForSkinTypeRepository ProductForSkinTypes { get; }
    IQuizSetRepository QuizSets { get; }
    IQuizQuestionRepository QuizQuestions { get; }
    IQuizOptionRepository QuizOptions { get; }
    IQuizResultRepository QuizResults { get; }
    ICountryRepository Countries { get; }


    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}