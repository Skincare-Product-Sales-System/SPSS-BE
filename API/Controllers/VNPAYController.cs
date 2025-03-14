using BusinessObjects.Dto.VariationOption;
using BusinessObjects.Dto.VNPay;
using Microsoft.AspNetCore.Mvc;
using Services.Dto.Api;
using Services.Interface;

namespace API.Controllers;
[ApiController]
[Route("api/VNPAY")]
public class VNPAYController : ControllerBase
    {
        private readonly IVNPayService _VNPAYService;

        public VNPAYController(IVNPayService vNPAYService)
        {
            _VNPAYService = vNPAYService;
        }
        //dùng để test
        [HttpGet("get-transaction-status-vnpay")]
       // [Authorize] //determine actor ???
        public async Task<IActionResult> GetTransactionStatusVNPay(Guid orderId, Guid userId, String urlReturn)
        {
            
            try
            {
                var vnpay = await _VNPAYService.GetTransactionStatusVNPay(orderId, userId, urlReturn);
                return Ok(ApiResponse<string>.SuccessResponse(vnpay));

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.FailureResponse(ex.Message));
            }
            // var response = new BaseResponse<string>
            // {
            //     Code = "Success",
            //     StatusCode = StatusCodeHelper.OK,
            //     Message = "Transaction created successfully.",
            //     Data = await _VNPAYService.GetTransactionStatusVNPay(orderId, userId, urlReturn)
            // };
            //
            // return Ok(response);
        }
        // Tự lấy param từ url và token, có thể dùng sau khi deploy
/*
        [HttpGet("get-transaction-status-vnpay")]
        [Authorize(Roles = "CUSTOMER")]
        public async Task<IActionResult> GetTransactionStatusVNPay([FromQuery] string orderId)
        {

            try
            {

                Guid userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var response = await _VNPAYService.GetTransactionStatusVNPay(orderId, userId, baseUrl);

                if (response != null)
                {
                    Redirect(response);
                }

                return Ok(response);
            }
            catch (Exception)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Bad Request");
            }
        }

*/
        [HttpGet("vnpay-payment")]
        public async Task<IActionResult> VNPAYPayment()
        {
            

            VNPAYRequest request = new()
            {
                VnpSecureHash = Request.Query["vnp_SecureHash"],
                VnpOrderInfo = Request.Query["vnp_OrderInfo"],
                VnpAmount = Request.Query["vnp_Amount"],
                VnpTransactionNo = Request.Query["vnp_TransactionNo"],
                VnpCardType = Request.Query["vnp_CardType"],
                VnpTransactionStatus = Request.Query["vnp_TransactionStatus"],
                VnpBankCode = Request.Query["vnp_BankCode"],
                VnpBankTranNo = Request.Query["vnp_BankTranNo"],
                VnpTxnRef = Request.Query["vnp_TxnRef"],
                VnpPayDate = Request.Query["vnp_PayDate"],
                VnpResponseCode = Request.Query["vnp_ResponseCode"],
                VnpTmnCode = Request.Query["vnp_TmnCode"]
            };


            var paymentStatus = await _VNPAYService.VNPAYPayment(request);
            //redirect về webpage của project, sau khi deploy set lại returnUrl trong service có thể mở ra

                return Redirect(paymentStatus.Text);



        }
    }