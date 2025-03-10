namespace Repositories.Interface;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICancelReasonRepository CancelReasons { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}