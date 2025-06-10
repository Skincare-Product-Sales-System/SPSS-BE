using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.SkinAnalysis;
using BusinessObjects.Dto.Transaction;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.Interface;
using Services.Dto.Api;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class SkinAnalysisService : ISkinAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FacePlusPlusClient _facePlusPlusClient;
        private readonly TensorFlowSkinAnalysisService _tensorFlowService;
        private readonly ManageFirebaseImage.ManageFirebaseImageService _firebaseImageService;
        private readonly ITransactionService _transactionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SkinAnalysisService>? _logger;
        
        // Read skin analysis cost from configuration
        private readonly decimal _skinAnalysisCost;

        public SkinAnalysisService(
            IUnitOfWork unitOfWork,
            FacePlusPlusClient facePlusPlusClient,
            TensorFlowSkinAnalysisService tensorFlowService,
            ITransactionService transactionService,
            IConfiguration configuration,
            ILogger<SkinAnalysisService>? logger = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _facePlusPlusClient = facePlusPlusClient ?? throw new ArgumentNullException(nameof(facePlusPlusClient));
            _tensorFlowService = tensorFlowService ?? throw new ArgumentNullException(nameof(tensorFlowService));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseImageService = new ManageFirebaseImage.ManageFirebaseImageService();
            _logger = logger;
            
            // Get skin analysis cost from configuration or use default value if not found
            if (!decimal.TryParse(_configuration["SkinAnalysis:Cost"], out _skinAnalysisCost))
            {
                _skinAnalysisCost = 20000; // Default value
                _logger?.LogWarning("SkinAnalysis:Cost not found in configuration. Using default value: {DefaultCost}", _skinAnalysisCost);
            }
        }

        /// <summary>
        /// Creates a payment request for skin analysis
        /// </summary>
        public async Task<TransactionDto> CreateSkinAnalysisPaymentRequestAsync(Guid userId)
        {
            try
            {
                _logger?.LogInformation("Creating skin analysis payment request for user {UserId}", userId);
                
                var createTransactionDto = new CreateTransactionDto
                {
                    TransactionType = "SkinAnalysis",
                    Amount = _skinAnalysisCost,
                    Description = "Thanh toan cho dich vu phan tich da"
                };
                
                var transaction = await _transactionService.CreateTransactionAsync(createTransactionDto, userId);
                
                _logger?.LogInformation("Skin analysis payment request created: {TransactionId}", transaction.Id);
                
                return transaction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating skin analysis payment request: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Checks payment status and proceeds with skin analysis if payment is approved
        /// </summary>
        public async Task<ApiResponse<object>> CheckPaymentStatusAndAnalyzeSkinAsync(IFormFile faceImage, Guid userId)
        {
            try
            {
                _logger?.LogInformation("Checking payment status for user {UserId}", userId);
                
                // Get the most recent pending transaction for this user
                var pendingTransactions = await _unitOfWork.Transactions.Entities
                    .Where(t => t.UserId == userId && 
                                t.TransactionType == "SkinAnalysis" && 
                                !t.IsDeleted)
                    .OrderByDescending(t => t.CreatedTime)
                    .ToListAsync();
                
                var latestTransaction = pendingTransactions.FirstOrDefault();
                
                if (latestTransaction == null)
                {
                    return ApiResponse<object>.FailureResponse("Không tìm thấy yêu cầu thanh toán cho dịch vụ phân tích da");
                }
                
                if (latestTransaction.Status == "Pending")
                {
                    return ApiResponse<object>.FailureResponse("Vui lòng thanh toán và chờ xác nhận từ admin");
                }
                
                if (latestTransaction.Status == "Rejected")
                {
                    return ApiResponse<object>.FailureResponse("Thanh toán của bạn đã bị từ chối. Vui lòng tạo yêu cầu thanh toán mới");
                }
                
                if (latestTransaction.Status == "Approved")
                {
                    // Process skin analysis
                    var result = await AnalyzeSkinAsync(faceImage, userId);
                    
                    // Mark transaction as used
                    latestTransaction.Description += " - Đã sử dụng";
                    _unitOfWork.Transactions.Update(latestTransaction);
                    await _unitOfWork.SaveChangesAsync();
                    
                    return ApiResponse<object>.SuccessResponse(result, "Phân tích da thành công");
                }
                
                return ApiResponse<object>.FailureResponse("Trạng thái thanh toán không hợp lệ");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking payment status: {ErrorMessage}", ex.Message);
                return ApiResponse<object>.FailureResponse("Lỗi khi kiểm tra trạng thái thanh toán", new List<string> { ex.Message });
            }
        }

        /// <summary>
        /// Analyzes skin from a face image and returns comprehensive skin analysis results
        /// </summary>
        /// <param name="faceImage">The facial image for analysis</param>
        /// <param name="userId">The ID of the user requesting the analysis</param>
        /// <returns>Comprehensive skin analysis results including condition, issues, and product recommendations</returns>
        public async Task<SkinAnalysisResultDto> AnalyzeSkinAsync(IFormFile faceImage, Guid userId)
        {
            try
            {
                _logger?.LogInformation("Starting skin analysis process for image {FileName}", faceImage.FileName);

                // 1. Upload image to Firebase for storage and reference
                string imageUrl = await UploadImageToFirebaseAsync(faceImage);
                _logger?.LogInformation("Image uploaded to Firebase: {ImageUrl}", imageUrl);

                // 2. Call Face++ API to analyze the skin
                var faceAnalysisResult = await _facePlusPlusClient.AnalyzeSkinAsync(faceImage);
                _logger?.LogInformation("Face++ analysis completed successfully");

                // 3. Parse the Face++ results
                var skinCondition = ExtractSkinCondition(faceAnalysisResult);
                var skinIssues = ExtractSkinIssues(faceAnalysisResult);
                _logger?.LogInformation("Extracted skin condition and issues from Face++ results");

                // 4. Use TensorFlow with EfficientNet for enhanced analysis
                var enhancedAnalysis = await _tensorFlowService.AnalyzeSkinAsync(faceImage, faceAnalysisResult);
                _logger?.LogInformation("TensorFlow analysis completed successfully");

                // 5. Determine skin type based on enhanced analysis
                var skinType = await DetermineSkinTypeAsync(skinCondition, enhancedAnalysis);
                skinCondition.SkinType = skinType.Name;
                _logger?.LogInformation("Determined skin type: {SkinType}", skinType.Name);

                // 6. Enhance skin issues with AI analysis
                skinIssues = EnhanceSkinIssues(skinIssues, enhancedAnalysis.EnhancedSkinIssues);
                _logger?.LogInformation("Enhanced skin issues with AI analysis. Found {IssueCount} issues", skinIssues.Count);

                // 7. Get product recommendations based on enhanced skin analysis
                var (recommendedProducts, routineSteps) =
                await GetEnhancedProductRecommendationsAsync(skinType.Id, skinIssues, enhancedAnalysis);
                _logger?.LogInformation("Generated {RecommendationCount} product recommendations", recommendedProducts.Count);

                // 8. Generate AI-enhanced skincare advice
                var skinCareAdvice = GenerateEnhancedSkinCareAdvice(skinType.Name, skinIssues, enhancedAnalysis);
                _logger?.LogInformation("Generated {AdviceCount} skincare advice items", skinCareAdvice.Count);

                // 9. Create and return the result
                var result = new SkinAnalysisResultDto
                {
                    Id = Guid.Empty, // Will be updated in SaveSkinAnalysisResultAsync
                    ImageUrl = imageUrl,
                    SkinCondition = skinCondition,
                    SkinIssues = skinIssues,
                    RecommendedProducts = recommendedProducts,
                    RoutineSteps = routineSteps,
                    SkinCareAdvice = skinCareAdvice,
                    CreatedTime = DateTimeOffset.UtcNow // Initialize with current time (will be updated in SaveSkinAnalysisResultAsync)
                };

                // 10. Save analysis results to database (this will set the Id and CreatedTime)
                await SaveSkinAnalysisResultAsync(result, userId, skinType.Id, enhancedAnalysis);
                _logger?.LogInformation("Saved skin analysis results to database with ID: {ResultId}", result.Id);

                _logger?.LogInformation("Skin analysis completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during skin analysis: {ErrorMessage}", ex.Message);
                throw new Exception("Skin analysis failed. Please try again later.", ex);
            }
        }

        /// <summary>
        /// Saves skin analysis results to the database
        /// </summary>
        private async Task SaveSkinAnalysisResultAsync(SkinAnalysisResultDto result, Guid userId, Guid skinTypeId, EnhancedSkinAnalysisDto enhancedAnalysis)
        {
            try
            {
                // Create the main skin analysis result entity
                var skinAnalysisResultId = Guid.NewGuid();
                result.Id = skinAnalysisResultId; // Set the ID in the DTO
                
                var skinAnalysisResult = new SkinAnalysisResult
                {
                    Id = skinAnalysisResultId,
                    ImageUrl = result.ImageUrl,
                    UserId = userId,
                    SkinTypeId = skinTypeId,
                    
                    // Skin condition scores
                    AcneScore = result.SkinCondition.AcneScore,
                    WrinkleScore = result.SkinCondition.WrinkleScore,
                    DarkCircleScore = result.SkinCondition.DarkCircleScore,
                    DarkSpotScore = result.SkinCondition.DarkSpotScore,
                    HealthScore = result.SkinCondition.HealthScore,
                    
                    // Enhanced analysis data
                    OilinessLevel = enhancedAnalysis.OilinessLevel,
                    DrynessLevel = enhancedAnalysis.DrynessLevel,
                    SensitivityLevel = enhancedAnalysis.SensitivityLevel,
                    
                    // Store full result as JSON
                    FullAnalysisJson = JsonConvert.SerializeObject(result),
                    
                    // Audit fields
                    CreatedBy = userId.ToString(),
                    CreatedTime = DateTimeOffset.UtcNow,
                    LastUpdatedBy = userId.ToString(),
                    LastUpdatedTime = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };

                // Set the creation time in the DTO
                result.CreatedTime = skinAnalysisResult.CreatedTime;

                // Add the main entity
                _unitOfWork.SkinAnalysisResults.Add(skinAnalysisResult);

                // Add skin issues
                foreach (var issue in result.SkinIssues)
                {
                    var skinIssueEntity = new SkinAnalysisIssue
                    {
                        Id = Guid.NewGuid(),
                        SkinAnalysisResultId = skinAnalysisResult.Id,
                        IssueName = issue.IssueName,
                        Description = issue.Description,
                        Severity = issue.Severity
                    };
                    
                    _unitOfWork.SkinAnalysisIssues.Add(skinIssueEntity);
                }

                // Add product recommendations
                foreach (var recommendation in result.RecommendedProducts)
                {
                    var recommendationEntity = new SkinAnalysisRecommendation
                    {
                        Id = Guid.NewGuid(),
                        SkinAnalysisResultId = skinAnalysisResult.Id,
                        ProductId = recommendation.ProductId,
                        RecommendationReason = recommendation.RecommendationReason,
                        PriorityScore = recommendation.PriorityScore
                    };
                    
                    _unitOfWork.SkinAnalysisRecommendations.Add(recommendationEntity);
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync();
                
                _logger?.LogInformation("Skin analysis result saved with ID: {ResultId}", result.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving skin analysis results: {ErrorMessage}", ex.Message);
                // We don't want to fail the whole analysis if saving to DB fails
                // So just log the error but don't rethrow
            }
        }

        /// <summary>
        /// Retrieves all skin analysis results with optional filtering and pagination
        /// </summary>
        public async Task<PagedResponse<SkinAnalysisResultDto>> GetAllSkinAnalysisResultsAsync(
            int pageNumber,
            int pageSize,
            string skinType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                _logger?.LogInformation("Retrieving all skin analysis results - Page: {PageNumber}, Size: {PageSize}, " +
                    "SkinType: {SkinType}, FromDate: {FromDate}, ToDate: {ToDate}",
                    pageNumber, pageSize, skinType, fromDate, toDate);

                // Start with the base query
                var query = _unitOfWork.SkinAnalysisResults.Entities
                    .Include(sar => sar.SkinType)
                    .Include(sar => sar.User)
                    .Where(sar => !sar.IsDeleted);

                // Apply filters if provided
                if (!string.IsNullOrEmpty(skinType))
                {
                    query = query.Where(sar => sar.SkinType.Name.Contains(skinType));
                }

                if (fromDate.HasValue)
                {
                    var fromDateUtc = new DateTimeOffset(fromDate.Value.Date, TimeSpan.Zero);
                    query = query.Where(sar => sar.CreatedTime >= fromDateUtc);
                }

                if (toDate.HasValue)
                {
                    var toDateUtc = new DateTimeOffset(toDate.Value.Date.AddDays(1), TimeSpan.Zero); // Include the entire day
                    query = query.Where(sar => sar.CreatedTime < toDateUtc);
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply paging
                var skip = (pageNumber - 1) * pageSize;
                var results = await query
                    .OrderByDescending(sar => sar.CreatedTime)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert to DTOs
                var resultDtos = new List<SkinAnalysisResultDto>();
                foreach (var result in results)
                {
                    try
                    {
                        // First try to deserialize from stored JSON if available
                        if (!string.IsNullOrEmpty(result.FullAnalysisJson))
                        {
                            var dto = JsonConvert.DeserializeObject<SkinAnalysisResultDto>(result.FullAnalysisJson);

                            // Add user information for admin view
                            dto.UserId = result.UserId;
                            dto.UserName = result.User?.UserName ?? "Unknown";
                            dto.CreatedTime = result.CreatedTime;

                            resultDtos.Add(dto);
                        }
                        else
                        {
                            // If JSON not available, get the full result by ID
                            var dto = await GetSkinAnalysisResultByIdAsync(result.Id);

                            // Add user information for admin view
                            dto.UserId = result.UserId;
                            dto.UserName = result.User?.UserName ?? "Unknown";
                            dto.CreatedTime = result.CreatedTime;

                            resultDtos.Add(dto);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error retrieving skin analysis result {ResultId}: {ErrorMessage}",
                            result.Id, ex.Message);
                        // Continue with next result if one fails
                    }
                }

                // Create paged response
                var pagedResponse = new PagedResponse<SkinAnalysisResultDto>
                {
                    Items = resultDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                _logger?.LogInformation("Successfully retrieved {Count} skin analysis results (total: {TotalCount})",
                    resultDtos.Count, totalCount);

                return pagedResponse;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all skin analysis results: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Extracts skin condition metrics from Face++ analysis results
        /// </summary>
        private SkinConditionDto ExtractSkinCondition(Dictionary<string, object> facePlusPlusResult)
        {
            try
            {
                var skinCondition = new SkinConditionDto();

                // Extract skin status from Face++ result
                if (facePlusPlusResult.TryGetValue("faces", out var facesObj) && facesObj is JArray faces && faces.Count > 0)
                {
                    var firstFace = faces[0] as JObject;
                    if (firstFace != null &&
                        firstFace.TryGetValue("attributes", out var attributesToken) &&
                        attributesToken is JObject attributes)
                    {
                        if (attributes.TryGetValue("skinstatus", out var skinStatusToken) &&
                            skinStatusToken is JObject skinStatus)
                        {
                            // Extract scores from skin status
                            skinCondition.AcneScore = GetSkinValue(skinStatus, "acne", 0);
                            skinCondition.WrinkleScore = GetSkinValue(skinStatus, "wrinkle", 0);
                            skinCondition.DarkCircleScore = GetSkinValue(skinStatus, "dark_circle", 0);
                            skinCondition.DarkSpotScore = GetSkinValue(skinStatus, "spot", 0);

                            // Calculate overall health score (inverse of average issues)
                            int totalIssues = skinCondition.AcneScore + skinCondition.WrinkleScore +
                                             skinCondition.DarkCircleScore + skinCondition.DarkSpotScore;
                            int avgIssues = totalIssues / 4;
                            skinCondition.HealthScore = Math.Max(0, 100 - avgIssues);
                        }
                    }
                }

                return skinCondition;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting skin condition: {ErrorMessage}", ex.Message);
                return new SkinConditionDto();
            }
        }

        /// <summary>
        /// Extracts skin issues from Face++ analysis results
        /// </summary>
        private List<SkinIssueDto> ExtractSkinIssues(Dictionary<string, object> facePlusPlusResult)
        {
            try
            {
                var skinIssues = new List<SkinIssueDto>();

                // Extract skin status from Face++ result
                if (facePlusPlusResult.TryGetValue("faces", out var facesObj) && facesObj is JArray faces && faces.Count > 0)
                {
                    var firstFace = faces[0] as JObject;
                    if (firstFace != null &&
                        firstFace.TryGetValue("attributes", out var attributesToken) &&
                        attributesToken is JObject attributes)
                    {
                        if (attributes.TryGetValue("skinstatus", out var skinStatusToken) &&
                            skinStatusToken is JObject skinStatus)
                        {
                            // Check for acne
                            int acneScore = GetSkinValue(skinStatus, "acne", 0);
                            if (acneScore > 40)
                            {
                                skinIssues.Add(new SkinIssueDto
                                {
                                    IssueName = "Mụn",
                                    Description = "Da của bạn đang có dấu hiệu bị mụn",
                                    Severity = acneScore / 10
                                });
                            }

                            // Check for wrinkles
                            int wrinkleScore = GetSkinValue(skinStatus, "wrinkle", 0);
                            if (wrinkleScore > 30)
                            {
                                skinIssues.Add(new SkinIssueDto
                                {
                                    IssueName = "Nếp nhăn",
                                    Description = "Da của bạn đang có dấu hiệu lão hóa và nếp nhăn",
                                    Severity = wrinkleScore / 10
                                });
                            }

                            // Check for dark circles
                            int darkCircleScore = GetSkinValue(skinStatus, "dark_circle", 0);
                            if (darkCircleScore > 30)
                            {
                                skinIssues.Add(new SkinIssueDto
                                {
                                    IssueName = "Thâm quầng mắt",
                                    Description = "Vùng da quanh mắt của bạn có dấu hiệu thâm quầng",
                                    Severity = darkCircleScore / 10
                                });
                            }

                            // Check for dark spots
                            int darkSpotScore = GetSkinValue(skinStatus, "spot", 0);
                            if (darkSpotScore > 30)
                            {
                                skinIssues.Add(new SkinIssueDto
                                {
                                    IssueName = "Đậm nâu/tàn nhang",
                                    Description = "Da của bạn có dấu hiệu tàn nhang hoặc đốm nâu",
                                    Severity = darkSpotScore / 10
                                });
                            }
                        }
                    }
                }

                return skinIssues;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting skin issues: {ErrorMessage}", ex.Message);
                return new List<SkinIssueDto>();
            }
        }

        /// <summary>
        /// Helper method to extract integer values from JObject with error handling
        /// </summary>
        private int GetSkinValue(JObject obj, string key, int defaultValue)
        {
            if (obj != null && obj.TryGetValue(key, out var token) && token is JValue value)
            {
                return value.ToObject<int>();
            }
            return defaultValue;
        }

        /// <summary>
        /// Determines skin type based on analysis results
        /// </summary>
        private async Task<(Guid Id, string Name)> DetermineSkinTypeAsync(SkinConditionDto skinCondition, EnhancedSkinAnalysisDto enhancedAnalysis)
        {
            try
            {
                // Sử dụng kết quả phân tích nâng cao để xác định loại da chính xác hơn
                string skinTypeName = enhancedAnalysis.EnhancedSkinType;

                // Tìm loại da trong database
                var skinType = await _unitOfWork.SkinTypes.Entities
                    .FirstOrDefaultAsync(st => st.Name.ToLower().Contains(skinTypeName.ToLower()));

                // Nếu không tìm thấy, quay về phương pháp xác định cũ
                if (skinType == null)
                {
                    if (skinCondition.AcneScore > 60)
                    {
                        skinTypeName = "Da dầu";
                    }
                    else if (skinCondition.AcneScore < 30)
                    {
                        skinTypeName = "Da khô";
                    }
                    else
                    {
                        skinTypeName = "Da hỗn hợp";
                    }

                    skinType = await _unitOfWork.SkinTypes.Entities
                        .FirstOrDefaultAsync(st => st.Name.ToLower().Contains(skinTypeName.ToLower()));
                }

                // Nếu vẫn không tìm thấy, mặc định về loại da đầu tiên
                if (skinType == null)
                {
                    skinType = await _unitOfWork.SkinTypes.Entities.FirstOrDefaultAsync();
                    if (skinType == null)
                    {
                        throw new Exception("No skin types found in the database");
                    }
                }

                return (skinType.Id, skinType.Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error determining skin type: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Enhances basic skin issues with advanced AI analysis
        /// </summary>
        private List<SkinIssueDto> EnhanceSkinIssues(List<SkinIssueDto> basicIssues, List<EnhancedSkinIssueDto> enhancedIssues)
        {
            try
            {
                // Chuyển đổi các vấn đề da nâng cao thành định dạng cơ bản
                var combinedIssues = new List<SkinIssueDto>();

                // Thêm các vấn đề da cơ bản
                combinedIssues.AddRange(basicIssues);

                // Thêm các vấn đề da nâng cao (loại bỏ trùng lặp)
                foreach (var enhancedIssue in enhancedIssues)
                {
                    // Nếu vấn đề chưa tồn tại trong danh sách cơ bản
                    if (!basicIssues.Any(bi => bi.IssueName == enhancedIssue.IssueName))
                    {
                        combinedIssues.Add(new SkinIssueDto
                        {
                            IssueName = enhancedIssue.IssueName,
                            Description = enhancedIssue.Description,
                            Severity = enhancedIssue.Severity
                        });
                    }
                }

                return combinedIssues;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error enhancing skin issues: {ErrorMessage}", ex.Message);
                return basicIssues; // Return original issues if enhancement fails
            }
        }

        /// <summary>
        /// Gets enhanced product recommendations based on skin type, issues, and AI analysis,
        /// organized by skincare routine steps
        /// </summary>
        private async Task<(List<ProductRecommendationDto> AllProducts, List<SkincareRoutineStepDto> RoutineSteps)>
            GetEnhancedProductRecommendationsAsync(Guid skinTypeId, List<SkinIssueDto> skinIssues, EnhancedSkinAnalysisDto enhancedAnalysis)
        {
            try
            {
                // Lấy danh sách sản phẩm theo loại da
                var skinTypeProducts = await _unitOfWork.ProductForSkinTypes.Entities
                    .Include(pfs => pfs.Product)
                        .ThenInclude(p => p.ProductImages)
                    .Include(pfs => pfs.Product)
                        .ThenInclude(p => p.ProductCategory)
                    .Where(pfs => pfs.SkinTypeId == skinTypeId)
                    .Select(pfs => pfs.Product)
                    .Take(15)  // Tăng số lượng sản phẩm tiềm năng
                    .ToListAsync();

                if (skinTypeProducts == null || !skinTypeProducts.Any())
                {
                    return (new List<ProductRecommendationDto>(), new List<SkincareRoutineStepDto>());
                }

                var recommendations = new List<ProductRecommendationDto>();
                var existingIssueNames = skinIssues.Select(si => si.IssueName).ToList();

                // Danh sách thành phần cần tránh từ phân tích nâng cao
                var ingredientsToAvoid = enhancedAnalysis.EnhancedSkinIssues
                    .SelectMany(esi => esi.AvoidIngredients)
                    .Distinct()
                    .ToList();

                // Danh sách thành phần được khuyến nghị từ phân tích nâng cao
                var recommendedIngredients = enhancedAnalysis.EnhancedSkinIssues
                    .SelectMany(esi => esi.RecommendedIngredients)
                    .Distinct()
                    .ToList();

                foreach (var product in skinTypeProducts)
                {
                    string reason = $"Sản phẩm phù hợp với loại da {enhancedAnalysis.EnhancedSkinType} của bạn";
                    int priorityScore = 0; // Điểm ưu tiên để sắp xếp sản phẩm

                    // Tăng điểm ưu tiên nếu sản phẩm phù hợp với vấn đề da
                    foreach (var issue in skinIssues)
                    {
                        string categoryLower = product.ProductCategory.CategoryName.ToLower();
                        string issueLower = issue.IssueName.ToLower();

                        if (categoryLower.Contains(issueLower) ||
                            (issueLower.Contains("mụn") && categoryLower.Contains("acne")) ||
                            (issueLower.Contains("nhăn") && (categoryLower.Contains("wrinkle") || categoryLower.Contains("anti-aging"))) ||
                            (issueLower.Contains("thâm") && categoryLower.Contains("dark")) ||
                            (issueLower.Contains("đỏ") && categoryLower.Contains("soothing")))
                        {
                            priorityScore += issue.Severity * 2;
                            reason += $" và giúp cải thiện {issue.IssueName.ToLower()}";
                        }
                    }

                    // Kiểm tra thành phần sản phẩm
                    if (!string.IsNullOrEmpty(product.Description))
                    {
                        // Tăng điểm ưu tiên nếu sản phẩm chứa thành phần được khuyến nghị
                        foreach (var ingredient in recommendedIngredients)
                        {
                            if (product.Description.ToLower().Contains(ingredient.ToLower()))
                            {
                                priorityScore += 5;
                                reason += $", chứa thành phần {ingredient} tốt cho da bạn";
                                break; // Chỉ đề cập một thành phần trong lý do
                            }
                        }

                        // Giảm điểm ưu tiên nếu sản phẩm chứa thành phần nên tránh
                        foreach (var ingredient in ingredientsToAvoid)
                        {
                            if (product.Description.ToLower().Contains(ingredient.ToLower()))
                            {
                                priorityScore -= 10;
                                // Không đề cập trong lý do, chỉ giảm điểm
                                break;
                            }
                        }
                    }

                    // Thêm sản phẩm với mức điểm ưu tiên
                    recommendations.Add(new ProductRecommendationDto
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        ImageUrl = product.ProductImages.FirstOrDefault(pi => pi.IsThumbnail)?.ImageUrl ?? "",
                        Price = product.Price,
                        RecommendationReason = reason,
                        PriorityScore = priorityScore
                    });
                }

                // Lọc sản phẩm có điểm dương và sắp xếp theo điểm ưu tiên
                var filteredRecommendations = recommendations
                    .Where(r => r.PriorityScore >= 0)
                    .OrderByDescending(r => r.PriorityScore)
                    .Take(10)
                    .ToList();

                // Lấy các bước chăm sóc da theo loại da
                var routineSteps = await _unitOfWork.SkinTypeRoutineSteps.Entities
                    .Include(s => s.Category)
                    .Where(s => s.SkinTypeId == skinTypeId)
                    .OrderBy(s => s.Order)
                    .ToListAsync();

                // Tạo cấu trúc các bước chăm sóc da kèm sản phẩm gợi ý
                var routineStepsDto = new List<SkincareRoutineStepDto>();

                if (routineSteps.Any())
                {
                    // Nếu có các bước routine được định nghĩa sẵn, sử dụng chúng
                    foreach (var step in routineSteps)
                    {
                        var stepDto = new SkincareRoutineStepDto
                        {
                            StepName = step.StepName,
                            Instruction = step.Instruction,
                            Order = step.Order,
                            Products = filteredRecommendations
                                .Where(p =>
                                    // Lọc sản phẩm theo danh mục phù hợp với bước chăm sóc
                                    skinTypeProducts
                                        .FirstOrDefault(sp => sp.Id == p.ProductId)?.ProductCategoryId == step.CategoryId)
                                .OrderByDescending(p => p.PriorityScore)
                                .ToList()
                        };

                        routineStepsDto.Add(stepDto);
                    }
                }
                else
                {
                    // Nếu không có routine được định nghĩa, tạo một bộ cơ bản dựa trên các danh mục phổ biến
                    var defaultSteps = new[]
                    {
                new { Name = "Sữa rửa mặt", Order = 1, Instruction = "Rửa mặt với sữa rửa mặt phù hợp loại da", Categories = new[] { "cleanser", "làm sạch", "rửa mặt" } },
                new { Name = "Toner", Order = 2, Instruction = "Cân bằng độ pH với toner", Categories = new[] { "toner", "nước hoa hồng" } },
                new { Name = "Serum/Điều trị", Order = 3, Instruction = "Thoa serum điều trị các vấn đề da cụ thể", Categories = new[] { "serum", "điều trị", "treatment", "essence" } },
                new { Name = "Kem dưỡng ẩm", Order = 4, Instruction = "Dưỡng ẩm để giữ nước cho da", Categories = new[] { "moisturizer", "kem dưỡng", "dưỡng ẩm" } },
                new { Name = "Kem chống nắng", Order = 5, Instruction = "Bảo vệ da khỏi tia UV (chỉ sử dụng ban ngày)", Categories = new[] { "sunscreen", "chống nắng", "spf" } }
            };

                    foreach (var step in defaultSteps)
                    {
                        var stepProducts = filteredRecommendations
                            .Where(p => {
                                var product = skinTypeProducts.FirstOrDefault(sp => sp.Id == p.ProductId);
                                if (product == null) return false;

                                var categoryName = product.ProductCategory?.CategoryName?.ToLower() ?? "";
                                return step.Categories.Any(c => categoryName.Contains(c));
                            })
                            .OrderByDescending(p => p.PriorityScore)
                            .ToList();

                        if (stepProducts.Any())
                        {
                            routineStepsDto.Add(new SkincareRoutineStepDto
                            {
                                StepName = step.Name,
                                Instruction = step.Instruction,
                                Order = step.Order,
                                Products = stepProducts
                            });
                        }
                    }
                }

                return (filteredRecommendations, routineStepsDto);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting product recommendations: {ErrorMessage}", ex.Message);
                return (new List<ProductRecommendationDto>(), new List<SkincareRoutineStepDto>());
            }
        }
        /// <summary>
        /// Uploads image to Firebase storage and returns the URL
        /// </summary>
        private async Task<string> UploadImageToFirebaseAsync(IFormFile faceImage)
        {
            try
            {
                using var stream = faceImage.OpenReadStream();
                var fileName = $"skin-analysis-{Guid.NewGuid()}_{faceImage.FileName}";
                return await _firebaseImageService.UploadFileAsync(stream, fileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error uploading image to Firebase: {ErrorMessage}", ex.Message);
                throw new Exception("Failed to upload image. Please try again later.", ex);
            }
        }

        /// <summary>
        /// Generates enhanced skincare advice based on skin type, issues, and AI analysis
        /// </summary>
        private List<string> GenerateEnhancedSkinCareAdvice(string skinType, List<SkinIssueDto> skinIssues, EnhancedSkinAnalysisDto enhancedAnalysis)
        {
            try
            {
                var advice = new List<string>();

                // Lời khuyên cơ bản dựa trên loại da
                switch (skinType.ToLower())
                {
                    case "da dầu":
                        advice.Add("Sử dụng sữa rửa mặt dịu nhẹ có độ pH thấp dành riêng cho da dầu");
                        advice.Add("Tránh các sản phẩm chứa dầu và chất béo, ưu tiên sản phẩm ghi \"oil-free\" hoặc \"non-comedogenic\"");
                        advice.Add("Sử dụng toner chứa BHA để kiểm soát dầu và làm sạch sâu lỗ chân lông");
                        advice.Add("Dùng kem dưỡng ẩm dạng gel hoặc lotion nhẹ không chứa dầu");
                        break;

                    case "da khô":
                        advice.Add("Sử dụng sữa rửa mặt dạng cream hoặc lotion không chứa sulfate");
                        advice.Add("Thêm serum cấp ẩm chứa hyaluronic acid và ceramides vào quy trình chăm sóc");
                        advice.Add("Sử dụng kem dưỡng ẩm giàu dưỡng chất và chất béo lành tính");
                        advice.Add("Cân nhắc dùng dầu dưỡng vào buổi tối để khóa ẩm");
                        break;

                    case "da hỗn hợp":
                        advice.Add("Sử dụng sữa rửa mặt cân bằng pH không chứa sulfate mạnh");
                        advice.Add("Áp dụng phương pháp \"multi-masking\" - đắp mặt nạ khác nhau cho từng vùng da");
                        advice.Add("Dùng toner không cồn cho toàn bộ khuôn mặt");
                        advice.Add("Sử dụng kem dưỡng nhẹ cho vùng chữ T và kem đậm đặc hơn cho hai má");
                        break;

                    case "da nhạy cảm":
                        advice.Add("Sử dụng sữa rửa mặt không hương liệu và cực kỳ dịu nhẹ");
                        advice.Add("Tránh tất cả sản phẩm chứa cồn, hương liệu, và các chất kích ứng");
                        advice.Add("Thử nghiệm sản phẩm mới trên vùng da nhỏ trước khi sử dụng toàn mặt");
                        advice.Add("Ưu tiên các sản phẩm có ít thành phần và được thiết kế cho da nhạy cảm");
                        break;

                    default:
                        advice.Add("Duy trì quy trình chăm sóc da cơ bản: làm sạch, dưỡng ẩm, chống nắng");
                        break;
                }

                // Lời khuyên dựa trên các vấn đề da cụ thể từ phân tích nâng cao
                foreach (var enhancedIssue in enhancedAnalysis.EnhancedSkinIssues)
                {
                    switch (enhancedIssue.IssueName)
                    {
                        case "Mụn":
                            advice.Add($"Sử dụng sản phẩm chứa {string.Join(", ", enhancedIssue.RecommendedIngredients)} để giảm mụn");
                            advice.Add($"Tránh sản phẩm chứa {string.Join(", ", enhancedIssue.AvoidIngredients)} vì có thể làm tắc nghẽn lỗ chân lông");
                            advice.Add("Rửa mặt hai lần mỗi ngày và sau khi đổ mồ hôi nhiều");
                            break;

                        case "Nếp nhăn":
                            advice.Add($"Thêm {string.Join(", ", enhancedIssue.RecommendedIngredients)} vào quy trình chăm sóc da buổi tối");
                            advice.Add("Massage nhẹ nhàng khi thoa sản phẩm để tăng cường tuần hoàn máu");
                            advice.Add("Cân nhắc sử dụng mặt nạ giàu collagen và peptide 1-2 lần/tuần");
                            break;

                        case "Thâm quầng mắt":
                            advice.Add($"Sử dụng kem mắt chứa {string.Join(", ", enhancedIssue.RecommendedIngredients)}");
                            advice.Add("Đảm bảo ngủ đủ 7-8 tiếng mỗi đêm và uống đủ nước");
                            advice.Add("Sử dụng miếng đắp mắt mát lạnh để giảm bọng mắt và thâm quầng");
                            break;

                        case "Đậm nâu/tàn nhang":
                            advice.Add($"Sử dụng serum làm sáng da với {string.Join(", ", enhancedIssue.RecommendedIngredients)}");
                            advice.Add("Đeo kính râm và mũ rộng vành khi ra ngoài để bảo vệ da khỏi tia UV");
                            advice.Add("Sử dụng kem chống nắng phổ rộng (broad spectrum) có SPF 50 trở lên");
                            break;

                        case "Lỗ chân lông to":
                            advice.Add($"Sử dụng sản phẩm chứa {string.Join(", ", enhancedIssue.RecommendedIngredients)} để làm se khít lỗ chân lông");
                            advice.Add("Rửa mặt bằng nước mát sau khi làm sạch để giúp se khít lỗ chân lông");
                            advice.Add("Đắp mặt nạ đất sét 1-2 lần/tuần để hút dầu và làm sạch sâu");
                            break;

                        case "Da đỏ/kích ứng":
                            advice.Add($"Sử dụng sản phẩm chứa {string.Join(", ", enhancedIssue.RecommendedIngredients)} để làm dịu da");
                            advice.Add("Tránh sử dụng nước quá nóng khi rửa mặt");
                            advice.Add("Tránh các sản phẩm tẩy tế bào chết hóa học khi da đang bị kích ứng");
                            break;

                        case "Da nhạy cảm":
                            advice.Add($"Chọn sản phẩm có {string.Join(", ", enhancedIssue.RecommendedIngredients)} để tăng cường hàng rào bảo vệ da");
                            advice.Add($"Tránh xa các sản phẩm chứa {string.Join(", ", enhancedIssue.AvoidIngredients)}");
                            advice.Add("Giảm tần suất sử dụng các sản phẩm hoạt tính mạnh như retinol và AHA");
                            break;
                    }
                }

                advice.Add("Uống đủ 2 lít nước mỗi ngày và duy trì chế độ ăn giàu chất chống oxy hóa");
                advice.Add("Thay vỏ gối ít nhất một lần/tuần và tránh chạm tay vào mặt");

                // Cá nhân hóa lời khuyên theo các chỉ số cụ thể
                if (enhancedAnalysis.OilinessLevel > 70)
                {
                    advice.Add("Da bạn tiết dầu nhiều: Sử dụng giấy thấm dầu vào giữa ngày để kiểm soát độ bóng");
                }

                if (enhancedAnalysis.DrynessLevel > 70)
                {
                    advice.Add("Da bạn thiếu ẩm trầm trọng: Cân nhắc sử dụng máy tạo độ ẩm trong phòng khi ngủ");
                }

                if (enhancedAnalysis.SensitivityLevel > 70)
                {
                    advice.Add("Da bạn rất nhạy cảm: Cân nhắc gặp bác sĩ da liễu để tư vấn chuyên sâu");
                }

                advice.Add("Nếu có dấu hiệu xấu hơn, hãy tham khảo ý kiến chuyên gia da liễu");

                return advice;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating skincare advice: {ErrorMessage}", ex.Message);
                return new List<string> { "Duy trì quy trình chăm sóc da cơ bản: làm sạch, dưỡng ẩm, chống nắng" };
            }
        }

        /// <summary>
        /// Retrieves a specific skin analysis result by ID
        /// </summary>
        public async Task<SkinAnalysisResultDto> GetSkinAnalysisResultByIdAsync(Guid id)
        {
            var result = await _unitOfWork.SkinAnalysisResults.Entities
                .Include(sar => sar.SkinType)
                .Include(sar => sar.User)
                .Include(sar => sar.SkinAnalysisIssues)
                .Include(sar => sar.SkinAnalysisRecommendations)
                    .ThenInclude(sr => sr.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(sar => sar.Id == id && !sar.IsDeleted);

            if (result == null)
            {
                throw new KeyNotFoundException($"Skin analysis result with ID {id} not found");
            }

            // If we have the full JSON result stored, we can deserialize it directly
            if (!string.IsNullOrEmpty(result.FullAnalysisJson))
            {
                try
                {
                    var dto = JsonConvert.DeserializeObject<SkinAnalysisResultDto>(result.FullAnalysisJson);
                    // Add the ID and creation time since they might not be in the serialized JSON
                    dto.Id = result.Id;
                    dto.CreatedTime = result.CreatedTime;
                    return dto;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error deserializing stored skin analysis JSON: {ErrorMessage}", ex.Message);
                    // Continue with manual mapping below if deserialization fails
                }
            }

            // Otherwise, manually construct the DTO from the database entities
            var skinCondition = new SkinConditionDto
            {
                AcneScore = result.AcneScore,
                WrinkleScore = result.WrinkleScore,
                DarkCircleScore = result.DarkCircleScore,
                DarkSpotScore = result.DarkSpotScore,
                HealthScore = result.HealthScore,
                SkinType = result.SkinType?.Name
            };

            var skinIssues = result.SkinAnalysisIssues.Select(issue => new SkinIssueDto
            {
                IssueName = issue.IssueName,
                Description = issue.Description,
                Severity = issue.Severity
            }).ToList();

            var recommendedProducts = result.SkinAnalysisRecommendations.Select(rec => new ProductRecommendationDto
            {
                ProductId = rec.ProductId,
                Name = rec.Product?.Name,
                Description = rec.Product?.Description,
                ImageUrl = rec.Product?.ProductImages.FirstOrDefault(pi => pi.IsThumbnail)?.ImageUrl ?? "",
                Price = rec.Product?.Price ?? 0,
                RecommendationReason = rec.RecommendationReason,
                PriorityScore = rec.PriorityScore
            }).ToList();

            return new SkinAnalysisResultDto
            {
                Id = result.Id, // Include the ID
                ImageUrl = result.ImageUrl,
                SkinCondition = skinCondition,
                SkinIssues = skinIssues,
                RecommendedProducts = recommendedProducts,
                CreatedTime = result.CreatedTime, // Include creation time
                // Since we don't store skin care advice separately, we'll return an empty list
                SkinCareAdvice = new List<string>()
            };
        }

        /// <summary>
        /// Retrieves all skin analysis results for a specific user
        /// </summary>
        public async Task<List<SkinAnalysisResultDto>> GetSkinAnalysisResultsByUserIdAsync(Guid userId)
        {
            var results = await _unitOfWork.SkinAnalysisResults.Entities
                .Include(sar => sar.SkinType)
                .Where(sar => sar.UserId == userId && !sar.IsDeleted)
                .OrderByDescending(sar => sar.CreatedTime)
                .ToListAsync();

            var resultDtos = new List<SkinAnalysisResultDto>();

            foreach (var result in results)
            {
                try
                {
                    var dto = await GetSkinAnalysisResultByIdAsync(result.Id);
                    resultDtos.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error retrieving skin analysis result {ResultId}: {ErrorMessage}", result.Id, ex.Message);
                    // Continue with next result if one fails
                }
            }

            return resultDtos;
        }

        /// <summary>
        /// Retrieves paged skin analysis results for a specific user
        /// </summary>
        /// <param name="userId">User ID to retrieve skin analysis results for</param>
        /// <param name="pageNumber">Page number, starting from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged response containing skin analysis results</returns>
        public async Task<PagedResponse<SkinAnalysisResultDto>> GetPagedSkinAnalysisResultsByUserIdAsync(
            Guid userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

                // Query to get total count
                var totalCount = await _unitOfWork.SkinAnalysisResults.Entities
                    .Where(sar => sar.UserId == userId && !sar.IsDeleted)
                    .CountAsync();

                // Query to get paged results with minimal data to improve performance
                var pagedResults = await _unitOfWork.SkinAnalysisResults.Entities
                    .Include(r => r.SkinType)
                    .Where(sar => sar.UserId == userId && !sar.IsDeleted)
                    .OrderByDescending(sar => sar.CreatedTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.ImageUrl,
                        r.CreatedTime,
                        r.SkinTypeId,
                        SkinTypeName = r.SkinType.Name,
                        r.HealthScore
                    })
                    .ToListAsync();

                // Map to DTOs with minimal information for the list view
                var items = pagedResults.Select(r => new SkinAnalysisResultDto
                {
                    Id = r.Id,
                    ImageUrl = r.ImageUrl,
                    CreatedTime = r.CreatedTime,
                    SkinCondition = new SkinConditionDto
                    {
                        HealthScore = r.HealthScore,
                        SkinType = r.SkinTypeName
                    },
                    // Other properties are initialized as empty collections
                    SkinIssues = new List<SkinIssueDto>(),
                    RecommendedProducts = new List<ProductRecommendationDto>(),
                    RoutineSteps = new List<SkincareRoutineStepDto>(),
                    SkinCareAdvice = new List<string>()
                }).ToList();

                _logger?.LogInformation("Retrieved {Count} paged skin analysis results for user {UserId}", items.Count, userId);

                return new PagedResponse<SkinAnalysisResultDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paged skin analysis results for user {UserId}: {ErrorMessage}", userId, ex.Message);
                throw;
            }
        }
    }
}