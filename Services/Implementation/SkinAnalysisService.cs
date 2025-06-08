using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.SkinAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly ManageFirebaseImage.ManageFirebaseImageService _firebaseImageService;

        public SkinAnalysisService(
            IUnitOfWork unitOfWork,
            FacePlusPlusClient facePlusPlusClient)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _facePlusPlusClient = facePlusPlusClient ?? throw new ArgumentNullException(nameof(facePlusPlusClient));
            _firebaseImageService = new ManageFirebaseImage.ManageFirebaseImageService();
        }

        public async Task<SkinAnalysisResultDto> AnalyzeSkinAsync(IFormFile faceImage)
        {
            // 1. Upload image to Firebase for storage and reference
            string imageUrl = await UploadImageToFirebaseAsync(faceImage);
            
            // 2. Call Face++ API to analyze the skin
            var faceAnalysisResult = await _facePlusPlusClient.AnalyzeSkinAsync(faceImage);
            
            // 3. Parse the Face++ results
            var skinCondition = ExtractSkinCondition(faceAnalysisResult);
            var skinIssues = ExtractSkinIssues(faceAnalysisResult);
            
            // 4. Determine skin type based on analysis
            var skinType = await DetermineSkinTypeAsync(skinCondition);
            skinCondition.SkinType = skinType.Name;
            
            // 5. Get product recommendations based on skin type and issues
            var recommendedProducts = await GetProductRecommendationsAsync(skinType.Id, skinIssues);
            
            // 6. Generate skincare advice
            var skinCareAdvice = GenerateSkinCareAdvice(skinType.Name, skinIssues);
            
            // 7. Create and return the result
            return new SkinAnalysisResultDto
            {
                ImageUrl = imageUrl,
                SkinCondition = skinCondition,
                SkinIssues = skinIssues,
                RecommendedProducts = recommendedProducts,
                SkinCareAdvice = skinCareAdvice
            };
        }

        private async Task<string> UploadImageToFirebaseAsync(IFormFile faceImage)
        {
            using var stream = faceImage.OpenReadStream();
            var fileName = $"skin-analysis-{Guid.NewGuid()}_{faceImage.FileName}";
            return await _firebaseImageService.UploadFileAsync(stream, fileName);
        }

        private SkinConditionDto ExtractSkinCondition(Dictionary<string, object> faceAnalysisResult)
        {
            // Parse the Face++ API response to extract skin condition data
            if (faceAnalysisResult.TryGetValue("faces", out var facesObj) && facesObj is JArray faces && faces.Count > 0)
            {
                var firstFace = faces[0] as JObject;
                if (firstFace != null && 
                    firstFace.TryGetValue("attributes", out var attributesToken) && 
                    attributesToken is JObject attributes)
                {
                    if (attributes.TryGetValue("skinstatus", out var skinStatusToken) && 
                        skinStatusToken is JObject skinStatus)
                    {
                        return new SkinConditionDto
                        {
                            AcneScore = GetIntValue(skinStatus, "acne", 0),
                            WrinkleScore = GetIntValue(skinStatus, "wrinkle", 0),
                            DarkCircleScore = GetIntValue(skinStatus, "dark_circle", 0),
                            DarkSpotScore = GetIntValue(skinStatus, "spot", 0),
                            HealthScore = CalculateHealthScore(skinStatus)
                        };
                    }
                }
            }
            
            // Return default values if we couldn't extract the data
            return new SkinConditionDto
            {
                AcneScore = 0,
                WrinkleScore = 0,
                DarkCircleScore = 0,
                DarkSpotScore = 0,
                HealthScore = 50 // Default middle value
            };
        }

        private List<SkinIssueDto> ExtractSkinIssues(Dictionary<string, object> faceAnalysisResult)
        {
            var issues = new List<SkinIssueDto>();
            
            if (faceAnalysisResult.TryGetValue("faces", out var facesObj) && facesObj is JArray faces && faces.Count > 0)
            {
                var firstFace = faces[0] as JObject;
                if (firstFace != null && 
                    firstFace.TryGetValue("attributes", out var attributesToken) && 
                    attributesToken is JObject attributes)
                {
                    if (attributes.TryGetValue("skinstatus", out var skinStatusToken) && 
                        skinStatusToken is JObject skinStatus)
                    {
                        // Map Face++ results to skin issues
                        if (GetIntValue(skinStatus, "acne", 0) > 40)
                        {
                            issues.Add(new SkinIssueDto
                            {
                                IssueName = "Mụn",
                                Description = "Da của bạn đang có dấu hiệu bị mụn",
                                Severity = GetIntValue(skinStatus, "acne", 0) / 10
                            });
                        }
                        
                        if (GetIntValue(skinStatus, "wrinkle", 0) > 30)
                        {
                            issues.Add(new SkinIssueDto
                            {
                                IssueName = "Nếp nhăn",
                                Description = "Da của bạn đang có dấu hiệu lão hóa và nếp nhăn",
                                Severity = GetIntValue(skinStatus, "wrinkle", 0) / 10
                            });
                        }
                        
                        if (GetIntValue(skinStatus, "dark_circle", 0) > 30)
                        {
                            issues.Add(new SkinIssueDto
                            {
                                IssueName = "Thâm quầng mắt",
                                Description = "Vùng da quanh mắt của bạn có dấu hiệu thâm quầng",
                                Severity = GetIntValue(skinStatus, "dark_circle", 0) / 10
                            });
                        }
                        
                        if (GetIntValue(skinStatus, "spot", 0) > 30)
                        {
                            issues.Add(new SkinIssueDto
                            {
                                IssueName = "Đậm nâu/tàn nhang",
                                Description = "Da của bạn có dấu hiệu tàn nhang do ánh nắng mặt trời",
                                Severity = GetIntValue(skinStatus, "spot", 0) / 10
                            });
                        }
                    }
                }
            }
            
            return issues;
        }

        private async Task<(Guid Id, string Name)> DetermineSkinTypeAsync(SkinConditionDto skinCondition)
        {
            // Simple logic to determine skin type based on analysis
            string skinTypeName;
            
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
            
            // Find skin type ID from database
            var skinType = await _unitOfWork.SkinTypes.Entities
                .FirstOrDefaultAsync(st => st.Name.ToLower().Contains(skinTypeName.ToLower()));
            
            // If skin type not found, default to first available
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

        private async Task<List<ProductRecommendationDto>> GetProductRecommendationsAsync(Guid skinTypeId, List<SkinIssueDto> skinIssues)
        {
            // Get products by skin type
            var skinTypeProducts = await _unitOfWork.ProductForSkinTypes.Entities
                .Include(pfs => pfs.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(pfs => pfs.Product)
                    .ThenInclude(p => p.ProductCategory)
                .Where(pfs => pfs.SkinTypeId == skinTypeId)
                .Select(pfs => pfs.Product)
                .Take(10)  // Limit to 10 products
                .ToListAsync();

            if (skinTypeProducts == null || !skinTypeProducts.Any())
            {
                return new List<ProductRecommendationDto>();
            }

            var recommendations = new List<ProductRecommendationDto>();
            
            foreach (var product in skinTypeProducts)
            {
                string reason = $"Sản phẩm phù hợp với loại da của bạn";
                
                // Add specific reasons based on skin issues
                if (skinIssues.Any(i => i.IssueName == "Mụn") && 
                    product.ProductCategory.CategoryName.Contains("Mụn"))
                {
                    reason += " và giúp điều trị mụn";
                }
                else if (skinIssues.Any(i => i.IssueName == "Nếp nhăn") && 
                         product.ProductCategory.CategoryName.Contains("Chống lão hóa"))
                {
                    reason += " và giúp giảm nếp nhăn";
                }
                
                recommendations.Add(new ProductRecommendationDto
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl = product.ProductImages.FirstOrDefault(pi => pi.IsThumbnail)?.ImageUrl ?? "",
                    Price = product.Price,
                    RecommendationReason = reason
                });
            }
            
            return recommendations;
        }

        private List<string> GenerateSkinCareAdvice(string skinType, List<SkinIssueDto> skinIssues)
        {
            var advice = new List<string>();
            
            // Basic advice based on skin type
            switch (skinType.ToLower())
            {
                case "da dầu":
                    advice.Add("Sử dụng sữa rửa mặt dành cho da dầu");
                    advice.Add("Tránh các sản phẩm chứa dầu, ưu tiên sản phẩm dành cho 'không gây mụn'");
                    advice.Add("Sử dụng kem dưỡng ẩm không chứa dầu");
                    break;
                    
                case "da khô":
                    advice.Add("Sử dụng sữa rửa mặt dịu nhẹ, không chứa sulfate");
                    advice.Add("Thêm serum cấp ẩm vào quy trình chăm sóc");
                    advice.Add("Sử dụng kem dưỡng ẩm giàu dưỡng chất");
                    break;

                case "da hỗn hợp":
                    advice.Add("Sử dụng sữa rửa mặt dịu nhẹ, cân bằng pH");
                    advice.Add("Dùng sản phẩm khác nhau cho vùng chữ T và hai má");
                    advice.Add("Cân bằng độ ẩm với kem dưỡng phù hợp");
                    break;
                    
                default:
                    advice.Add("Duy trì quy trình chăm sóc da cơ bản: làm sạch, dưỡng ẩm, chống nắng");
                    break;
            }
            
            // Add advice for specific issues
            foreach (var issue in skinIssues)
            {
                switch (issue.IssueName)
                {
                    case "Mụn":
                        advice.Add("Sử dụng sản phẩm chứa BHA (salicylic acid) để giảm mụn");
                        advice.Add("Tránh chạm tay vào mặt và thường xuyên thay vỏ gối");
                        break;
                        
                    case "Nếp nhăn":
                        advice.Add("Thêm retinol vào quy trình chăm sóc da buổi tối");
                        advice.Add("Sử dụng kem chống nắng mỗi ngày để ngăn tình trạng lão hóa sớm");
                        break;
                        
                    case "Thâm quầng mắt":
                        advice.Add("Sử dụng kem mắt chứa caffeine và vitamin K");
                        advice.Add("Đảm bảo ngủ đủ giấc và uống nhiều nước");
                        break;
                        
                    case "Đậm nâu/tàn nhang":
                        advice.Add("Sử dụng sản phẩm chứa vitamin C để làm sáng da");
                        advice.Add("Luôn bôi kem chống nắng có SPF 30 trở lên");
                        break;
                }
            }
            
            // General advice for everyone
            advice.Add("Uống đủ 2 lít nước mỗi ngày");
            advice.Add("Luôn bôi kem chống nắng mỗi ngày, kể cả khi ở trong nhà");
            
            advice.Add("Thường xuyên kiểm tra tình trạng da để điều chỉnh sản phẩm chăm sóc phù hợp");
            
            advice.Add("Nếu có dấu hiệu xấu hơn, hãy tham khảo ý kiến chuyên gia da liễu");

            return advice;
        }

        private int GetIntValue(JObject obj, string key, int defaultValue)
        {
            if (obj != null && obj.TryGetValue(key, out var token) && token is JValue value)
            {
                return value.ToObject<int>();
            }
            return defaultValue;
        }

        private int CalculateHealthScore(JObject skinStatus)
        {
            if (skinStatus == null)
            {
                return 50; // Default middle value if no data
            }

            // Calculate a health score based on various skin metrics
            // Lower values of issues = higher health score
            int acneScore = GetIntValue(skinStatus, "acne", 0);
            int wrinkleScore = GetIntValue(skinStatus, "wrinkle", 0);
            int darkCircleScore = GetIntValue(skinStatus, "dark_circle", 0);
            int spotScore = GetIntValue(skinStatus, "spot", 0);
            
            // Reverse the scores (higher is better for health score)
            int reversedSum = 400 - (acneScore + wrinkleScore + darkCircleScore + spotScore);
            
            // Convert to 0-100 scale
            return Math.Max(0, Math.Min(100, reversedSum / 4));
        }
    }
}