using AutoMapper;
using BusinessObjects.Dto.QuizQuestion;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class QuizQuestionService : IQuizQuestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuizQuestionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<QuizQuestionDto> GetByIdAsync(Guid id)
    {
        var quizQuestion = await _unitOfWork.QuizQuestions.GetByIdAsync(id);
        if (quizQuestion == null || quizQuestion.IsDeleted)
            throw new KeyNotFoundException($"QuizQuestion with ID {id} not found.");
        return _mapper.Map<QuizQuestionDto>(quizQuestion);
    }

  public async Task<PagedResponse<QuizQuestionDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (quizQuestions, totalCount) = await _unitOfWork.QuizQuestions.GetPagedAsync(
            pageNumber, pageSize, q => q.IsDeleted == false);
    
        var quizQuestionDtos = quizQuestions.Select(q => new QuizQuestionDto
        {
            Id = q.Id,
            SetId = q.QuizSetId,
            Value = q.Value
        }).ToList();
    
        return new PagedResponse<QuizQuestionDto>
        {
            Items = quizQuestionDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<QuizQuestionDto> CreateAsync(QuizQuestionForCreationDto? quizQuestionForCreationDto)
    {
        if (quizQuestionForCreationDto is null)
            throw new ArgumentNullException(nameof(quizQuestionForCreationDto), "QuizQuestion data cannot be null.");

        var quizQuestion = _mapper.Map<QuizQuestion>(quizQuestionForCreationDto);
        quizQuestion.Id = Guid.NewGuid();
        quizQuestion.CreatedTime = DateTimeOffset.UtcNow;
        quizQuestion.CreatedBy = "System"; // You can replace "System" with actual user context
        quizQuestion.IsDeleted = false;

        _unitOfWork.QuizQuestions.Add(quizQuestion);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizQuestionDto>(quizQuestion);
    }

    public async Task<QuizQuestionDto> UpdateAsync(Guid quizQuestionId, QuizQuestionForUpdateDto quizQuestionForUpdateDto)
    {
        if (quizQuestionForUpdateDto is null)
            throw new ArgumentNullException(nameof(quizQuestionForUpdateDto), "QuizQuestion data cannot be null.");

        var quizQuestion = await _unitOfWork.QuizQuestions.GetByIdAsync(quizQuestionId);
        if (quizQuestion == null || quizQuestion.IsDeleted)
            throw new KeyNotFoundException($"QuizQuestion with ID {quizQuestionId} not found.");

        quizQuestion.LastUpdatedTime = DateTimeOffset.UtcNow;
        quizQuestion.LastUpdatedBy = "System"; // You can replace "System" with actual user context

        _mapper.Map(quizQuestionForUpdateDto, quizQuestion);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizQuestionDto>(quizQuestion);
    }

    public async Task DeleteAsync(Guid id)
    {
        var quizQuestion = await _unitOfWork.QuizQuestions.GetByIdAsync(id);
        if (quizQuestion == null || quizQuestion.IsDeleted)
            throw new KeyNotFoundException($"QuizQuestion with ID {id} not found.");

        quizQuestion.IsDeleted = true;
        quizQuestion.DeletedTime = DateTimeOffset.UtcNow;
        quizQuestion.DeletedBy = "System"; // You can replace "System" with actual user context

        _unitOfWork.QuizQuestions.Update(quizQuestion);
        await _unitOfWork.SaveChangesAsync();
    }

 public async Task<IEnumerable<QuizQuestionDto>> GetByQuizSetIdAsync(Guid quizSetId)
 {
     var quizQuestions = await _unitOfWork.QuizQuestions.FindAsync(q => q.QuizSetId == quizSetId && !q.IsDeleted);
     var quizQuestionDtos = quizQuestions.Select(q => new QuizQuestionDto
     {
         Id = q.Id,
         SetId = q.QuizSetId,
         Value = q.Value
     }).ToList();
     return quizQuestionDtos;
 }
    public async Task<QuizQuestionDto> AddQuizQuestionToQuizSetAsync(Guid quizSetId, QuizQuestionForCreationDto quizQuestionForCreationDto)
    {
        if (quizQuestionForCreationDto is null)
            throw new ArgumentNullException(nameof(quizQuestionForCreationDto), "QuizQuestion data cannot be null.");

        var quizQuestion = _mapper.Map<QuizQuestion>(quizQuestionForCreationDto);
        quizQuestion.Id = Guid.NewGuid();
        quizQuestion.QuizSetId = quizSetId;
        quizQuestion.CreatedTime = DateTimeOffset.UtcNow;
        quizQuestion.CreatedBy = "System"; // You can replace "System" with actual user context
        quizQuestion.IsDeleted = false;
        _unitOfWork.QuizQuestions.Add(quizQuestion);
        await _unitOfWork.SaveChangesAsync();
        var quizQuestionDto = new QuizQuestionDto
        {
            Id = quizQuestion.Id,
            SetId = quizQuestion.QuizSetId,
            Value = quizQuestion.Value
        };
        return quizQuestionDto;
    }

    public async Task<QuizQuestionDto> UpdateQuizQuestionInQuizSetAsync(Guid quizSetId, Guid quizQuestionId, QuizQuestionForUpdateDto quizQuestionForUpdateDto)
    {
        if (quizQuestionForUpdateDto is null)
            throw new ArgumentNullException(nameof(quizQuestionForUpdateDto), "QuizQuestion data cannot be null.");

        var quizQuestion = await _unitOfWork.QuizQuestions.GetByIdAsync(quizQuestionId);
        if (quizQuestion == null || quizQuestion.IsDeleted || quizQuestion.QuizSetId != quizSetId)
            throw new KeyNotFoundException($"QuizQuestion with ID {quizQuestionId} not found in QuizSet with ID {quizSetId}.");

        quizQuestion.LastUpdatedTime = DateTimeOffset.UtcNow;
        quizQuestion.LastUpdatedBy = "System"; // You can replace "System" with actual user context

        _mapper.Map(quizQuestionForUpdateDto, quizQuestion);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizQuestionDto>(quizQuestion);
    }

    public async Task DeleteQuizQuestionFromQuizSetAsync(Guid quizSetId, Guid quizQuestionId)
    {
        var quizQuestion = await _unitOfWork.QuizQuestions.GetByIdAsync(quizQuestionId);
        if (quizQuestion == null || quizQuestion.IsDeleted || quizQuestion.QuizSetId != quizSetId)
            throw new KeyNotFoundException($"QuizQuestion with ID {quizQuestionId} not found in QuizSet with ID {quizSetId}.");

        quizQuestion.IsDeleted = true;
        quizQuestion.DeletedTime = DateTimeOffset.UtcNow;
        quizQuestion.DeletedBy = "System"; // You can replace "System" with actual user context

        _unitOfWork.QuizQuestions.Update(quizQuestion);
        await _unitOfWork.SaveChangesAsync();
    }
}