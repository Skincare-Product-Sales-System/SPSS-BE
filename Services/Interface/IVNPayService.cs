using BusinessObjects.Dto.VNPay;

namespace Services.Interface;

    public interface IVNPayService
    {
        Task<string> GetTransactionStatusVNPay(Guid orderId, Guid userId, String urlReturn);

        Task<VNPAYResponse> VNPAYPayment(VNPAYRequest request);

    }
