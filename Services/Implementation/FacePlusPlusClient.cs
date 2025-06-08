using BusinessObjects.Dto.SkinAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class FacePlusPlusClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _detectUrl;
        private readonly string _analyzeUrl;

        public FacePlusPlusClient(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _apiKey = configuration["FacePlusPlus:ApiKey"];
            _apiSecret = configuration["FacePlusPlus:ApiSecret"];
            _detectUrl = configuration["FacePlusPlus:DetectUrl"] ?? "https://api-us.faceplusplus.com/facepp/v3/detect";
            _analyzeUrl = configuration["FacePlusPlus:AnalyzeUrl"] ?? "https://api-us.faceplusplus.com/facepp/v3/face/analyze";

            Console.WriteLine($"API Key: {_apiKey}");
            Console.WriteLine($"API Secret: {_apiSecret}");
            Console.WriteLine($"Detect URL: {_detectUrl}");
            Console.WriteLine($"Analyze URL: {_analyzeUrl}");

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret))
            {
                throw new InvalidOperationException("Face++ API configuration is missing or incomplete");
            }
        }

        public async Task<Dictionary<string, object>> AnalyzeSkinAsync(IFormFile faceImage)
        {
            try
            {
                // Step 1: Detect face to get face_token
                string faceToken = await DetectFaceAsync(faceImage);

                // Step 2: Analyze face using the face_token
                return await AnalyzeFaceAsync(faceToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Skin analysis process failed: {ex.Message}", ex);
            }
        }

        private async Task<string> DetectFaceAsync(IFormFile faceImage)
        {
            try
            {
                // Thử cách khác để gửi tham số
                var queryString = new Dictionary<string, string>
        {
            { "api_key", _apiKey },
            { "api_secret", _apiSecret },
            { "return_attributes", "gender,age,skinstatus" }
        };

                var detectUrlWithQuery = $"{_detectUrl}?{string.Join("&", queryString.Select(kv => $"{kv.Key}={kv.Value}"))}";
                Console.WriteLine($"URL with query: {detectUrlWithQuery}");

                // Tạo MultipartFormDataContent chỉ cho file
                using var formData = new MultipartFormDataContent();

                // Add image file
                using var imageStream = faceImage.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);

                var imageContent = new ByteArrayContent(memoryStream.ToArray());
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                formData.Add(imageContent, "image_file", faceImage.FileName);

                Console.WriteLine("Sending detect request with URL parameters...");
                var response = await _httpClient.PostAsync(detectUrlWithQuery, formData);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Detect response: {jsonResponse}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorObj = JObject.Parse(jsonResponse);
                    string errorMsg = errorObj["error_message"]?.ToString() ?? "Unknown error";
                    throw new HttpRequestException($"Face detection failed: {errorMsg}");
                }

                // Extract face_token from response
                var responseObj = JObject.Parse(jsonResponse);
                var faces = responseObj["faces"] as JArray;

                if (faces == null || !faces.Any())
                {
                    throw new Exception("No face detected in the image");
                }

                string faceToken = faces[0]["face_token"].ToString();
                Console.WriteLine($"Face token: {faceToken}");

                return faceToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DetectFaceAsync: {ex.Message}");
                throw;
            }
        }

        private async Task<Dictionary<string, object>> AnalyzeFaceAsync(string faceToken)
        {
            try
            {
                // Thử cách khác để gửi tham số giống như DetectFaceAsync
                var queryString = new Dictionary<string, string>
        {
            { "api_key", _apiKey },
            { "api_secret", _apiSecret },
            { "face_tokens", faceToken },
            { "return_attributes", "gender,age,skinstatus" },
            { "return_landmark", "1" }
        };

                var analyzeUrlWithQuery = $"{_analyzeUrl}?{string.Join("&", queryString.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}";
                Console.WriteLine($"Analyze URL with query: {analyzeUrlWithQuery}");

                // Không cần body cho request này
                using var content = new StringContent("");

                Console.WriteLine("Sending analyze request with URL parameters...");
                var response = await _httpClient.PostAsync(analyzeUrlWithQuery, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Analyze response: {jsonResponse}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorObj = JObject.Parse(jsonResponse);
                    string errorMsg = errorObj["error_message"]?.ToString() ?? "Unknown error";
                    throw new HttpRequestException($"Face analysis failed: {errorMsg}");
                }

                return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AnalyzeFaceAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<FacePlusPlusResponseDto> AnalyzeSkinWithTypedResponseAsync(IFormFile faceImage)
        {
            var responseDictionary = await AnalyzeSkinAsync(faceImage);
            var jsonString = JsonConvert.SerializeObject(responseDictionary);
            return JsonConvert.DeserializeObject<FacePlusPlusResponseDto>(jsonString);
        }
    }
}