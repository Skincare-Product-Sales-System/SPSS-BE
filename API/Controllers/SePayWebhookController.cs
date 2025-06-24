using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Services.Interface;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/webhook/sepay")]
public class SePayWebhookController : ControllerBase
{
    private readonly IOrderService _orderService;

    public SePayWebhookController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] SePayWebhookDto data)
    {
        // Tìm mã đơn hàng dạng Guid chuẩn hoặc 32 ký tự hexa không dấu gạch ngang
        var match = Regex.Match(data.content ?? "", @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})|([0-9a-fA-F]{32})");
        if (!match.Success)
            return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ" });

        Guid orderId;
        if (match.Value.Length == 32)
            orderId = Guid.ParseExact(match.Value, "N");
        else
            orderId = Guid.Parse(match.Value);

        // Lấy thông tin đơn hàng
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
            return BadRequest(new { success = false, message = "Không tìm thấy đơn hàng" });

        // Kiểm tra số tiền
        if (order.DiscountedOrderTotal != data.transferAmount)
            return BadRequest(new { success = false, message = "Sai số tiền" });

        // Cập nhật trạng thái đơn hàng sang PROCESSING, truyền Guid.Empty
        await _orderService.UpdateOrderStatusAsync(orderId, "Processing", Guid.Empty);

        return StatusCode(201, new { success = true });
    }
}

public class SePayWebhookDto
{
    public int id { get; set; }
    public string content { get; set; }
    public int transferAmount { get; set; }
    // ... các trường khác nếu cần
} 