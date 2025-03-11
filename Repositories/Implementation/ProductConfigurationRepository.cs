using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class ProductConfigurationRepository : RepositoryBase<ProductConfiguration, Guid>, IProductConfigurationRepository
    {
        public ProductConfigurationRepository(SPSSContext context) : base(context)
        {
        }
    }
}
