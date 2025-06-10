using BusinessObjects.Dto.SkinAnalysis;
using BusinessObjects.Dto.Transaction;
using Microsoft.AspNetCore.Http;
using Services.Dto.Api;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISkinAnalysisService
    {
        Task<SkinAnalysisResultDto> AnalyzeSkinAsync(IFormFile faceImage, Guid userId);
        Task<SkinAnalysisResultDto> GetSkinAnalysisResultByIdAsync(Guid id);
        Task<List<SkinAnalysisResultDto>> GetSkinAnalysisResultsByUserIdAsync(Guid userId);
        
        // Methods for payment-based skin analysis
        Task<TransactionDto> CreateSkinAnalysisPaymentRequestAsync(Guid userId);
        Task<ApiResponse<object>> CheckPaymentStatusAndAnalyzeSkinAsync(IFormFile faceImage, Guid userId);
        Task<PagedResponse<SkinAnalysisResultDto>> GetAllSkinAnalysisResultsAsync(
            int pageNumber,
            int pageSize,
            string skinType,
            DateTime? fromDate,
            DateTime? toDate);
    }
}