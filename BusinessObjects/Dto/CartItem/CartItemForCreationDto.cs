using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.CartItem
{
    public class CartItemForCreationDto
    {
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; }
    }
}
