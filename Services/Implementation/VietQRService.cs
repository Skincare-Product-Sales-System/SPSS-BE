using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

public class VietQRService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _apiKey;

    public VietQRService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _clientId = configuration["VietQR:ClientId"];
        _apiKey = configuration["VietQR:ApiKey"];
    }

    public async Task<string> GenerateQR(string accountNo, string accountName, int acqId, int amount, string addInfo, string template = "compact")
    {
        var url = "https://api.vietqr.io/v2/generate";
        var body = new
        {
            accountNo,
            accountName,
            acqId,
            amount,
            addInfo,
            template
        };
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-client-id", _clientId);
        request.Headers.Add("x-api-key", _apiKey);
        request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        if (json["code"]?.ToString() == "00")
        {
            return json["data"]?["qrDataURL"]?.ToString();
        }
        return null;
    }
} 