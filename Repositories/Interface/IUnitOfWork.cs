namespace Repositories.Interface;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IProductImageRepository ProductImages { get; }
    ICancelReasonRepository CancelReasons { get; }
    IProductCategoryRepository  ProductCategories { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}