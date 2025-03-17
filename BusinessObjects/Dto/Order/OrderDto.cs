using BusinessObjects.Dto.OrderDetail;

namespace BusinessObjects.Dto.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }

        public Guid PaymentMethodId { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }
}
