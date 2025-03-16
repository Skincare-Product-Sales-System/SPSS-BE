using AutoMapper;
using BusinessObjects.Dto.Brand;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.QuizResult;
using BusinessObjects.Dto.SkincareRoutinStep;
using BusinessObjects.Dto.SkinType;
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

        public async Task<QuizResultDto> GetByPointAndSetIdAsync(string score, Guid quizSetId, Guid userId)
        {
            // Lấy QuizResult và SkinType
            var quizResult = await _unitOfWork.QuizResults.Entities
                .Include(q => q.SkinType)
                .FirstOrDefaultAsync(q => q.Score == score && q.QuizSetId == quizSetId);

            if (quizResult == null)
                throw new KeyNotFoundException($"QuizResult with Score {score} and QuizSetId {quizSetId} not found.");

            // Gán SkinTypeId cho User
            var user = await _unitOfWork.Users.Entities.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new KeyNotFoundException($"User with Id {userId} not found.");
            user.SkinTypeId = quizResult.SkinTypeId;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Lấy RoutineSteps từ SkinTypeRoutineStep
            var routineSteps = _unitOfWork.SkinTypeRoutineSteps.Entities
                .Include(rs => rs.Category)
                .Where(rs => rs.SkinTypeId == quizResult.SkinTypeId)
                .Select(rs => new SkinTypeRoutineStepDto
                {
                    StepName = rs.StepName,
                    Category = new ProductCategoryOverviewDto
                    {
                        Id = rs.Category.Id,
                        CategoryName = rs.Category.CategoryName
                    }, // Lấy tên danh mục
                    Instruction = rs.Instruction,
                    Order = rs.Order,
                    Products = _unitOfWork.Products.Entities
                        .Where(p => p.ProductCategoryId == rs.CategoryId)
                        .OrderByDescending(p => p.SoldCount) // Ưu tiên sản phẩm bán chạy
                        .ThenByDescending(p => p.CreatedTime) // Ưu tiên sản phẩm mới
                        .Take(5) // Lấy tối đa 5 sản phẩm
                        .Select(p => new ProductForQuizResultDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Thumbnail = p.ProductImages.Where(p => p.IsThumbnail).Select(p => p.ImageUrl).FirstOrDefault(),
                            Price = p.Price,
                            Description = p.Description,
                            SoldCount = p.SoldCount
                        })
                        .ToList()
                })
                .ToList();

            // Mapping kết quả vào DTO
            var quizResultDto = new QuizResultDto
            {
                Id = quizResult.Id,
                Score = quizResult.Score,
                SkinTypeId = quizResult.SkinTypeId,
                Name = quizResult.SkinType?.Name,
                Description = quizResult.SkinType?.Description,
                Routine = routineSteps
            };

            return quizResultDto;
        }
    }
}
