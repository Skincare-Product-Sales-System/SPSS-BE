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
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public bool InStock { get; set; }
        public decimal Price { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal TotalPrice => Quantity * Price;
        public List<string> VariationOptionValues { get; set; } = new List<string>();
        public DateTimeOffset LastUpdatedTime { get; set; }
    }

}
