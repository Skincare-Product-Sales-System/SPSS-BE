using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class BlogRepository : RepositoryBase<Blog, Guid>, IBlogRepository
{
    public BlogRepository(SPSSContext context) : base(context)
    {
    }
}