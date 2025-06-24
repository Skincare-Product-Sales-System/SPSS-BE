using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class VietQRController : ControllerBase
{
    private readonly VietQRService _vietQRService;

    public VietQRController(VietQRService vietQRService)
    {
        _vietQRService = vietQRService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateQR([FromBody] VietQRRequestDto dto)
    {
        var qrDataUrl = await _vietQRService.GenerateQR(dto.AccountNo, dto.AccountName, dto.AcqId, dto.Amount, dto.AddInfo, dto.Template);
        if (qrDataUrl == null)
            return BadRequest("Không tạo được mã QR");
        return Ok(new { qrDataUrl });
    }
}

public class VietQRRequestDto
{
    public string AccountNo { get; set; }
    public string AccountName { get; set; }
    public int AcqId { get; set; }
    public int Amount { get; set; }
    public string AddInfo { get; set; }
    public string Template { get; set; } = "compact";
} 