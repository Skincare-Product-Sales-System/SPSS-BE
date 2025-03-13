using BusinessObjects.Dto.QuizSet;
using BusinessObjects.Dto.Review;
using BusinessObjects.Dto.User;
using Services.Dto.Api;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IQuizSetService
    {
        Task<PagedResponse<QuizSetQuestionAndAnswerDto>> GetQuizSetWithQuestionsAsync(Guid quizSetId, int pageNumber, int pageSize);

        Task<PagedResponse<QuizSetDto>> GetPagedAsync(int pageNumber, int pageSize);
    }
}
