using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class ReviewImageRepository : RepositoryBase<ReviewImage, Guid>, IReviewImageRepository
    {
        public ReviewImageRepository(SPSSContext context) : base(context)
        {
        }
    }
}
