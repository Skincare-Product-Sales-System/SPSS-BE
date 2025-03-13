using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class ProductItemRepository : RepositoryBase<ProductItem, Guid>, IProductItemRepository
    {
        public ProductItemRepository(SPSSContext context) : base(context)
        {
        }
    }
}
