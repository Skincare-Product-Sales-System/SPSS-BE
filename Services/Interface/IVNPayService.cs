namespace Services.Interface;

    public interface IVNPayService
    {
        Task<string> GetTransactionStatusVNPay(string orderId, Guid userId, String urlReturn);

        Task<VNPAYResponse> VNPAYPayment(VNPAYRequest request);

    }
