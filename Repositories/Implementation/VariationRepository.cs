using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class VariationRepository : RepositoryBase<Variation, Guid>, IVariationRepository
    {
        public VariationRepository(SPSSContext context) : base(context)
        {
        }
    }
}
