using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class ProductRepository : RepositoryBase<Product, Guid>, IProductRepository
{
    public ProductRepository(spss1Context context) : base(context)
    {
    }
}