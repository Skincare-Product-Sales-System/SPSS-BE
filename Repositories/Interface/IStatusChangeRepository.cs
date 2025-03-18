using BusinessObjects.Models;
using System.Linq.Expressions;

namespace Repositories.Interface
{
    public interface IStatusChangeRepository : IRepositoryBase<StatusChange, Guid>
    {
        Task<StatusChange> FirstOrDefaultAsync(Expression<Func<StatusChange, bool>> predicate);
    }
}
