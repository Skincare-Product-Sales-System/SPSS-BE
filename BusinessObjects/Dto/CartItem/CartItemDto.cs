using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.CartItem
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

}
