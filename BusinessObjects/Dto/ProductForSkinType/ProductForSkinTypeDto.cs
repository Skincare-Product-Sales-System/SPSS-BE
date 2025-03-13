using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.SkinType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.ProductForSkinType
{
    public class ProductForSkinTypeDto
    {
        public Guid SkinTypeId { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }
}
