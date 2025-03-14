using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class PromotionTargetRepository : RepositoryBase<PromotionTarget, Guid>, IPromotionTargetRepository
{
    public PromotionTargetRepository(SPSSContext context) : base(context)
    {
    }
}