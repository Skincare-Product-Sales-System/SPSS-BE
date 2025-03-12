using System.Text.Json.Serialization;

namespace BusinessObjects.Dto.OrderDetail
{
    public class OrderDetailForCreationDto
    {
        public int Quantity { get; set; }
        public Guid ProductItemId { get; set; }
    }
}
