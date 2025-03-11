using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class VariationOptionRepository : RepositoryBase<VariationOption, Guid>, IVariationOptionRepository
    {
        public VariationOptionRepository(SPSSContext context) : base(context)
        {
        }
    }
}
