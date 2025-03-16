using BusinessObjects.Dto.Promotion;
using BusinessObjects.Dto.QuizOption;
using Services.Response;

namespace Services.Interface;

public interface IQuizOptionService
{
    Task<QuizOptionDto> GetByIdAsync(Guid id);
    Task<PagedResponse<QuizOptionDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<QuizOptionDto> CreateAsync(QuizOptionForCreationDto? quizOptionForCreationDto);
    Task<QuizOptionDto> UpdateAsync(Guid promotionId, QuizOptionForUpdateDto quizOptionForUpdateDto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<QuizOptionDto>> GetByQuizQuestionIdAsync(Guid quizQuestionId);
    Task<QuizOptionDto> AddQuizOptionToQuizQuestionAsync(Guid quizQuestionId, QuizOptionForCreationDto quizOptionForCreationDto);
    Task<QuizOptionDto> UpdateQuizOptionInQuizQuestionAsync(Guid quizQuestionId, Guid quizOptionId, QuizOptionForUpdateDto quizOptionForUpdateDto);
    Task DeleteQuizOptionFromQuizQuestionAsync(Guid quizQuestionId, Guid quizOptionId);
}