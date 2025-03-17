using BusinessObjects.Dto.ProductStatus;
using BusinessObjects.Dto.QuizResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IQuizResultService
    {
        Task<QuizResultDto> GetByPointAndSetIdAsync(string score, Guid quizSetId, Guid userId);
    }
}
