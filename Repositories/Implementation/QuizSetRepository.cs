using BusinessObjects.Models;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class QuizSetRepository : RepositoryBase<QuizSet, Guid>, IQuizSetRepository
    {
        public QuizSetRepository(SPSSContext context) : base(context)
        {
        }
    }
}
