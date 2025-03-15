using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.QuizResult;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class QuizResultService : IQuizResultService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuizResultService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<QuizResultDto> GetByPointAndSetIdAsync(string score, Guid quizSetId)
        {
            var quizResult = await _unitOfWork.QuizResults.Entities
                .Include(q => q.SkinType)
                .FirstOrDefaultAsync(q => q.Score == score && q.SetId == quizSetId);

            if (quizResult == null)
                throw new KeyNotFoundException($"QuizResult with Score {score} and QuizSetId {quizSetId} not found.");

            return _mapper.Map<QuizResultDto>(quizResult);
        }
    }
}
