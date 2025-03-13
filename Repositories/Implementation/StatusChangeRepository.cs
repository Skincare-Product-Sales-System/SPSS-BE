using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class StatusChangeRepository : RepositoryBase<StatusChange, Guid>, IStatusChangeRepository
    {
        public StatusChangeRepository(SPSSContext context) : base(context)
        {
        }
    }
}
