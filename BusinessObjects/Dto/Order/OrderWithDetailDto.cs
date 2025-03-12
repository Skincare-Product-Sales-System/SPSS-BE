using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.OrderDetail;

namespace BusinessObjects.Dto.Order
{
    public class OrderWithDetailDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public OrderDetailDto OrderDetail { get; set; }
        public AddressDto Address { get; set; }
    }
}
