using BusinessObjects.Models;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class QuizResultRepository : RepositoryBase<QuizResult, Guid>, IQuizResultRepository
    {
        public QuizResultRepository(SPSSContext context) : base(context)
        {
        }
    }
}
