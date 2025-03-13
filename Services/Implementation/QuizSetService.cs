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

namespace Services.Implementation
{
    public class QuizSetService : IQuizSetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuizSetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<QuizSetQuestionAndAnswerDto>> GetQuizSetWithQuestionsAsync(Guid quizSetId, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            // Lấy QuizSet trước
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

            // Lấy danh sách câu hỏi có trong quizSetId (phân trang)
            var questions = await _unitOfWork.QuizQuestions.Entities
                .Where(q => q.SetId == quizSetId)
                .OrderBy(q => q.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách đáp án cho các câu hỏi
            var questionIds = questions.Select(q => q.Id).ToList();
            var options = await _unitOfWork.QuizOptions.Entities
                .Where(opt => questionIds.Contains(opt.QuestionId))
                .ToListAsync();

            // Ánh xạ vào DTO
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

            // Trả về kết quả phân trang
            return new PagedResponse<QuizSetQuestionAndAnswerDto>
            {
                Items = new List<QuizSetQuestionAndAnswerDto> { result },
                TotalCount = await _unitOfWork.QuizQuestions.Entities.CountAsync(q => q.SetId == quizSetId),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
