using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class CancelReasonRepository : RepositoryBase<CancelReason, Guid>, ICancelReasonRepository
    {
        public CancelReasonRepository(SPSSContext context) : base(context)
        {
        }
    }
}
