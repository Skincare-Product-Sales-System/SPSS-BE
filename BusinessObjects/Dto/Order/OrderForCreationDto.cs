using BusinessObjects.Dto.OrderDetail;
using System.Text.Json.Serialization;

namespace BusinessObjects.Dto.Order
{
    public class OrderForCreationDto
    {
        public Guid AddressId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public Guid? VoucherId { get; set; }
        public List<OrderDetailForCreationDto> OrderDetail { get; set; }
    }
}
