using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.SkinAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Repositories.Interface;
using Services.Interface;
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
        private readonly ILogger<SkinAnalysisService>? _logger;

        public SkinAnalysisService(
            IUnitOfWork unitOfWork,
            FacePlusPlusClient facePlusPlusClient,
            TensorFlowSkinAnalysisService tensorFlowService,
            ILogger<SkinAnalysisService>? logger = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _facePlusPlusClient = facePlusPlusClient ?? throw new ArgumentNullException(nameof(facePlusPlusClient));
            _tensorFlowService = tensorFlowService ?? throw new ArgumentNullException(nameof(tensorFlowService));
            _firebaseImageService = new ManageFirebaseImage.ManageFirebaseImageService();
            _logger = logger;
        }

        /// <summary>
        /// Analyzes skin from a face image and returns comprehensive skin analysis results
        /// </summary>
        /// <param name="faceImage">The facial image for analysis</param>
        /// <returns>Comprehensive skin analysis results including condition, issues, and product recommendations</returns>
        public async Task<SkinAnalysisResultDto> AnalyzeSkinAsync(IFormFile faceImage)
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
                var recommendedProducts = await GetEnhancedProductRecommendationsAsync(skinType.Id, skinIssues, enhancedAnalysis);
                _logger?.LogInformation("Generated {RecommendationCount} product recommendations", recommendedProducts.Count);

                // 8. Generate AI-enhanced skincare advice
                var skinCareAdvice = GenerateEnhancedSkinCareAdvice(skinType.Name, skinIssues, enhancedAnalysis);
                _logger?.LogInformation("Generated {AdviceCount} skincare advice items", skinCareAdvice.Count);

                // 9. Create and return the result
                var result = new SkinAnalysisResultDto
                {
                    ImageUrl = imageUrl,
                    SkinCondition = skinCondition,
                    SkinIssues = skinIssues,
                    RecommendedProducts = recommendedProducts,
                    SkinCareAdvice = skinCareAdvice
                };

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
        /// Gets enhanced product recommendations based on skin type, issues, and AI analysis
        /// </summary>
        private async Task<List<ProductRecommendationDto>> GetEnhancedProductRecommendationsAsync(Guid skinTypeId, List<SkinIssueDto> skinIssues, EnhancedSkinAnalysisDto enhancedAnalysis)
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
                    return new List<ProductRecommendationDto>();
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

                    // Kiểm tra thành phần sản phẩm (giả định có trường Ingredients)
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
                        PriorityScore = priorityScore // Thêm trường này vào DTO
                    });
                }

                // Sắp xếp sản phẩm theo điểm ưu tiên và chỉ lấy 10 sản phẩm có điểm cao nhất
                return recommendations
                    .Where(r => r.PriorityScore >= 0) // Loại bỏ sản phẩm có thành phần nên tránh
                    .OrderByDescending(r => r.PriorityScore)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting product recommendations: {ErrorMessage}", ex.Message);
                return new List<ProductRecommendationDto>();
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
    }
}