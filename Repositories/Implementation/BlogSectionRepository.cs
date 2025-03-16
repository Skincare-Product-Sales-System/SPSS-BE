using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class BlogSectionRepository : RepositoryBase<BlogSection, Guid>, IBlogSectionRepository
    {
        public BlogSectionRepository(SPSSContext context) : base(context)
        {
        }
    }
}
