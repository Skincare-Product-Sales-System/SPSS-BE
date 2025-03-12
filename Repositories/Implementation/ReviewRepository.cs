using BusinessObjects.Models;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    class ReviewRepository : RepositoryBase<Review, Guid>, IReviewRepository
    {
        public ReviewRepository(SPSSContext context) : base(context)
        {
        }
    }
}
