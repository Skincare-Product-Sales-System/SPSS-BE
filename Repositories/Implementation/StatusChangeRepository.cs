using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System.Linq.Expressions;

namespace Repositories.Implementation
{
    public class StatusChangeRepository : RepositoryBase<StatusChange, Guid>, IStatusChangeRepository
    {
        public StatusChangeRepository(SPSSContext context) : base(context)
        {
        }

        public async Task<StatusChange> FirstOrDefaultAsync(Expression<Func<StatusChange, bool>> predicate)
        {
            return await _context.StatusChanges.FirstOrDefaultAsync(predicate);
        }
    }
}
