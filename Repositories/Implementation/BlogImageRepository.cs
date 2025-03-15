using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class BlogImageRepository : RepositoryBase<BlogImage, Guid>, IBlogImageRepository
    {
        public BlogImageRepository(SPSSContext context) : base(context)
        {
        }   
    }
}
