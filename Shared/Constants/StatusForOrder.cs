namespace Shared.Constants
{
    public class StatusForOrder
    {
        public static string Pending { get; } = "Pending";
        public static string Processing { get; } = "Processing";
        public static string Cancelled { get; } = "Cancelled";
        public static string AwaitingPayment { get; } = "Awaiting Payment";
        public static string Refunded { get; } = "Refunded";
        public static string Shipping { get; } = "Shipping";
        public static string Delivered { get; } = "Delivered";
        public static string Returned { get; } = "Returned";
        public static string RefundPending { get; } = "Refund Pending";
    }
}
