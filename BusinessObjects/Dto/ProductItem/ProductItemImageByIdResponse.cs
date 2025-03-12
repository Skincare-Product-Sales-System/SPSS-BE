using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.ProductItem
{
    public class ProductItemImageByIdResponse
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
    }
}
