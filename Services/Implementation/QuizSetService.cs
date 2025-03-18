using BusinessObjects.Dto.QuizSet;
using BusinessObjects.Dto.QuizQuestion;
using BusinessObjects.Dto.QuizOption;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Models;

namespace Services.Implementation
{
    public class QuizSetService : IQuizSetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuizSetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResponse<QuizSetQuestionAndAnswerDto>> GetQuizSetWithQuestionsAsync(Guid quizSetId, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            var quizSet = await _unitOfWork.QuizSets.Entities
                .Where(qs => qs.Id == quizSetId)
                .Select(qs => new
                {
                    qs.Id,
                    qs.Name
                })
                .FirstOrDefaultAsync();

            if (quizSet == null)
            {
                return new PagedResponse<QuizSetQuestionAndAnswerDto>
                {
                    Items = new List<QuizSetQuestionAndAnswerDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var questions = await _unitOfWork.QuizQuestions.Entities
                .Where(q => q.SetId == quizSetId)
                .OrderBy(q => q.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var questionIds = questions.Select(q => q.Id).ToList();
            var options = await _unitOfWork.QuizOptions.Entities
                .Where(opt => questionIds.Contains(opt.QuestionId))
                .ToListAsync();

            var result = new QuizSetQuestionAndAnswerDto
            {
                Id = quizSet.Id,
                QuizSetName = quizSet.Name,
                QuizQuestions = questions.Select(q => new QuizQuestionAndAnswerDto
                {
                    Id = q.Id,
                    Value = q.Value,
                    QuizOptions = options
                        .Where(opt => opt.QuestionId == q.Id)
                        .Select(opt => new QuizOptionDto
                        {
                            Id = opt.Id,
                            Value = opt.Value,
                            Score = opt.Score
                        }).ToList()
                }).ToList()
            };

            return new PagedResponse<QuizSetQuestionAndAnswerDto>
            {
                Items = new List<QuizSetQuestionAndAnswerDto> { result },
                TotalCount = await _unitOfWork.QuizQuestions.Entities.CountAsync(q => q.SetId == quizSetId),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponse<QuizSetDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (quizSets, totalCount) = await _unitOfWork.QuizSets.GetPagedAsync(
                pageNumber, pageSize, s => s.IsDeleted == false);

            var quizSetDtos = _mapper.Map<IEnumerable<QuizSetDto>>(quizSets);
            return new PagedResponse<QuizSetDto>
            {
                Items = quizSetDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<QuizSetDto> CreateAsync(QuizSetForCreationDto? quizSetForCreationDto)
        {
            if (quizSetForCreationDto is null)
                throw new ArgumentNullException(nameof(quizSetForCreationDto), "QuizSet data cannot be null.");

            var quizSet = _mapper.Map<QuizSet>(quizSetForCreationDto);
            quizSet.Id = Guid.NewGuid();
            quizSet.CreatedTime = DateTimeOffset.UtcNow;
            quizSet.CreatedBy = "System"; 
            quizSet.IsDeleted = false;

            _unitOfWork.QuizSets.Add(quizSet);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<QuizSetDto>(quizSet);
        }

        public async Task<QuizSetDto> UpdateAsync(Guid quizSetId, QuizSetForUpdateDto quizSetForUpdateDto)
        {
            if (quizSetForUpdateDto is null)
                throw new ArgumentNullException(nameof(quizSetForUpdateDto), "QuizSet data cannot be null.");

            var quizSet = await _unitOfWork.QuizSets.GetByIdAsync(quizSetId);
            if (quizSet == null || quizSet.IsDeleted)
                throw new KeyNotFoundException($"QuizSet with ID {quizSetId} not found.");

            quizSet.LastUpdatedTime = DateTimeOffset.UtcNow;
            quizSet.LastUpdatedBy = "System"; // You can replace "System" with actual user context

            _mapper.Map(quizSetForUpdateDto, quizSet);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<QuizSetDto>(quizSet);
        }

        public async Task DeleteAsync(Guid id)
        {
            var quizSet = await _unitOfWork.QuizSets.GetByIdAsync(id);
            if (quizSet == null || quizSet.IsDeleted)
                throw new KeyNotFoundException($"QuizSet with ID {id} not found.");

            quizSet.IsDeleted = true;
            quizSet.DeletedTime = DateTimeOffset.UtcNow;
            quizSet.DeletedBy = "System"; // You can replace "System" with actual user context

            _unitOfWork.QuizSets.Update(quizSet);
            await _unitOfWork.SaveChangesAsync();
        }
        
        public async Task SetQuizSetDefaultAsync(Guid quizSetId)
        {
            var quizSet = await _unitOfWork.QuizSets.GetByIdAsync(quizSetId);
            if (quizSet == null || quizSet.IsDeleted)
                throw new KeyNotFoundException($"QuizSet with ID {quizSetId} not found.");

            quizSet.IsDefault = true;
            _unitOfWork.QuizSets.Update(quizSet);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}