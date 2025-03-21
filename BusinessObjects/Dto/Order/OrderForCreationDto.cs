using BusinessObjects.Dto.OrderDetail;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BusinessObjects.Dto.Order
{
    public class OrderForCreationDto
    {
        [Required(ErrorMessage = "Address ID is required.")]
        public Guid AddressId { get; set; }

        [Required(ErrorMessage = "Payment method ID is required.")]
        public Guid PaymentMethodId { get; set; }

        public Guid? VoucherId { get; set; }

        [Required(ErrorMessage = "Order details are required.")]
        [MinLength(1, ErrorMessage = "At least one order detail is required.")]
        public List<OrderDetailForCreationDto> OrderDetail { get; set; }
    }
}
