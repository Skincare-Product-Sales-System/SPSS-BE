using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class BrandRepository : RepositoryBase<Brand, Guid>, IBrandRepository
{
    public BrandRepository(SPSSContext context) : base(context)
    {
    }
}