using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class PromotionRepository : RepositoryBase<Promotion, Guid>, IPromotionRepository
{
    public PromotionRepository(SPSSContext context) : base(context)
    {
    }
}