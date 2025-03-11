using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class ProductStatusRepository : RepositoryBase<ProductStatus ,Guid>, IProductStatusRepository
    {
        public ProductStatusRepository(SPSSContext context) : base(context)
        {
        }
    }
}
