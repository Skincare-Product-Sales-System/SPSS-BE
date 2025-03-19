using BusinessObjects.Dto.QuizOption;
using BusinessObjects.Dto.QuizQuestion;
using Services.Response;

namespace Services.Interface;

public interface IQuizQuestionService
{
    Task<QuizQuestionDto> GetByIdAsync(Guid id);
    Task<PagedResponse<QuizQuestionDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<QuizQuestionDto> CreateAsync(QuizQuestionForCreationDto? quizOptionForCreationDto);
    Task<QuizQuestionDto> UpdateAsync(Guid id, QuizQuestionForUpdateDto quizOptionForUpdateDto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<QuizQuestionDto>> GetByQuizSetIdAsync(Guid quizSetId);
    Task<QuizQuestionDto> AddQuizQuestionToQuizSetAsync(Guid quizSetId, QuizQuestionForCreationDto quizQuestionForCreationDto);
    Task<QuizQuestionDto> UpdateQuizQuestionInQuizSetAsync(Guid quizSetId, Guid quizQuestionId, QuizQuestionForUpdateDto quizQuestionForUpdateDto);
    Task DeleteQuizQuestionFromQuizSetAsync(Guid quizSetId, Guid quizQuestionId);
}