using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.OrderDetail;
using BusinessObjects.Dto.StatusChange;

namespace BusinessObjects.Dto.Order
{
    public class OrderWithDetailDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public Guid? CancelReasonId { get; set; }    
        public DateTimeOffset? CreatedTime { get; set; }
        public Guid PaymentMethodId { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
        public AddressDto Address { get; set; }
        public List<StatusChangeDto> StatusChanges { get; set; }
    }
}
