using BusinessObjects.Models;
using Repositories.Implementation;
using Repositories.Interface;

namespace Repositories
{
    public class BlogSectionRepository : RepositoryBase<BlogSection, Guid>, IBlogSectionRepository
    {
        public BlogSectionRepository(SPSSContext context) : base(context)
        {
        }
    }
}
