using AutoMapper;
using BusinessObjects.Dto.QuizOption;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class QuizOptionService : IQuizOptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuizOptionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<QuizOptionDto> GetByIdAsync(Guid id)
    {
        var quizOption = await _unitOfWork.QuizOptions.GetByIdAsync(id);
        if (quizOption == null || quizOption.IsDeleted)
            throw new KeyNotFoundException($"QuizOption with ID {id} not found.");
        return _mapper.Map<QuizOptionDto>(quizOption);
    }

    public async Task<PagedResponse<QuizOptionDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (quizOptions, totalCount) = await _unitOfWork.QuizOptions.GetPagedAsync(
            pageNumber, pageSize, q => q.IsDeleted == false);

        var quizOptionDtos = _mapper.Map<IEnumerable<QuizOptionDto>>(quizOptions);
        return new PagedResponse<QuizOptionDto>
        {
            Items = quizOptionDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<QuizOptionDto> CreateAsync(QuizOptionForCreationDto? quizOptionForCreationDto)
    {
        if (quizOptionForCreationDto is null)
            throw new ArgumentNullException(nameof(quizOptionForCreationDto), "QuizOption data cannot be null.");

        var quizOption = _mapper.Map<QuizOption>(quizOptionForCreationDto);
        quizOption.Id = Guid.NewGuid();
        quizOption.CreatedTime = DateTimeOffset.UtcNow;
        quizOption.CreatedBy = "System"; // You can replace "System" with actual user context
        quizOption.IsDeleted = false;

        _unitOfWork.QuizOptions.Add(quizOption);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizOptionDto>(quizOption);
    }

    public async Task<QuizOptionDto> UpdateAsync(Guid quizOptionId, QuizOptionForUpdateDto quizOptionForUpdateDto)
    {
        if (quizOptionForUpdateDto is null)
            throw new ArgumentNullException(nameof(quizOptionForUpdateDto), "QuizOption data cannot be null.");

        var quizOption = await _unitOfWork.QuizOptions.GetByIdAsync(quizOptionId);
        if (quizOption == null || quizOption.IsDeleted)
            throw new KeyNotFoundException($"QuizOption with ID {quizOptionId} not found.");

        quizOption.LastUpdatedTime = DateTimeOffset.UtcNow;
        quizOption.LastUpdatedBy = "System"; // You can replace "System" with actual user context

        _mapper.Map(quizOptionForUpdateDto, quizOption);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizOptionDto>(quizOption);
    }

    public async Task DeleteAsync(Guid id)
    {
        var quizOption = await _unitOfWork.QuizOptions.GetByIdAsync(id);
        if (quizOption == null || quizOption.IsDeleted)
            throw new KeyNotFoundException($"QuizOption with ID {id} not found.");

        quizOption.IsDeleted = true;
        quizOption.DeletedTime = DateTimeOffset.UtcNow;
        quizOption.DeletedBy = "System"; // You can replace "System" with actual user context

        _unitOfWork.QuizOptions.Update(quizOption);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<QuizOptionDto>> GetByQuizQuestionIdAsync(Guid quizQuestionId)
    {
        var quizOptions = await _unitOfWork.QuizOptions.FindAsync(q => q.QuizQuestionId == quizQuestionId && !q.IsDeleted);
        return _mapper.Map<IEnumerable<QuizOptionDto>>(quizOptions);
    }

    public async Task<QuizOptionDto> AddQuizOptionToQuizQuestionAsync(Guid quizQuestionId, QuizOptionForCreationDto quizOptionForCreationDto)
    {
        if (quizOptionForCreationDto is null)
            throw new ArgumentNullException(nameof(quizOptionForCreationDto), "QuizOption data cannot be null.");

        var quizOption = _mapper.Map<QuizOption>(quizOptionForCreationDto);
        quizOption.Id = Guid.NewGuid();
        quizOption.QuizQuestionId = quizQuestionId;
        quizOption.CreatedTime = DateTimeOffset.UtcNow;
        quizOption.CreatedBy = "System"; // You can replace "System" with actual user context
        quizOption.IsDeleted = false;

        _unitOfWork.QuizOptions.Add(quizOption);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizOptionDto>(quizOption);
    }

    public async Task<QuizOptionDto> UpdateQuizOptionInQuizQuestionAsync(Guid quizQuestionId, Guid quizOptionId, QuizOptionForUpdateDto quizOptionForUpdateDto)
    {
        if (quizOptionForUpdateDto is null)
            throw new ArgumentNullException(nameof(quizOptionForUpdateDto), "QuizOption data cannot be null.");

        var quizOption = await _unitOfWork.QuizOptions.GetByIdAsync(quizOptionId);
        if (quizOption == null || quizOption.IsDeleted || quizOption.QuizQuestionId != quizQuestionId)
            throw new KeyNotFoundException($"QuizOption with ID {quizOptionId} not found in QuizQuestion with ID {quizQuestionId}.");

        quizOption.LastUpdatedTime = DateTimeOffset.UtcNow;
        quizOption.LastUpdatedBy = "System"; // You can replace "System" with actual user context

        _mapper.Map(quizOptionForUpdateDto, quizOption);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<QuizOptionDto>(quizOption);
    }

    public async Task DeleteQuizOptionFromQuizQuestionAsync(Guid quizQuestionId, Guid quizOptionId)
    {
        var quizOption = await _unitOfWork.QuizOptions.GetByIdAsync(quizOptionId);
        if (quizOption == null || quizOption.IsDeleted || quizOption.QuizQuestionId != quizQuestionId)
            throw new KeyNotFoundException($"QuizOption with ID {quizOptionId} not found in QuizQuestion with ID {quizQuestionId}.");

        quizOption.IsDeleted = true;
        quizOption.DeletedTime = DateTimeOffset.UtcNow;
        quizOption.DeletedBy = "System"; // You can replace "System" with actual user context

        _unitOfWork.QuizOptions.Update(quizOption);
        await _unitOfWork.SaveChangesAsync();
    }
}