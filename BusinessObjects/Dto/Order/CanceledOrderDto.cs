namespace BusinessObjects.Dto.Order
{
    public class CanceledOrderDto
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public decimal Total { get; set; }
        public DateTimeOffset? RefundTime { get; set; }
        public string RefundReason { get; set; }
        public decimal RefundRate { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
