using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class PromotionTypeRepository : RepositoryBase<PromotionType, Guid>, IPromotionTypeRepository
    {
        public PromotionTypeRepository(SPSSContext context) : base(context)
        {
        }
    }
}
