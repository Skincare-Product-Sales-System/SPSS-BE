using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.CartItem
{
    public class CartItemForCreationDto
    {
        public int UserId { get; set; }
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; }
        public string CreatedBy { get; set; }
    }
}
