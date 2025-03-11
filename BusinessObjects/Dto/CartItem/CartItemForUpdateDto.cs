using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.CartItem
{
    public class CartItemForUpdateDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
