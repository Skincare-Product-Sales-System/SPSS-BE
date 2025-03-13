using BusinessObjects.Dto.ProductForSkinType;
using BusinessObjects.Dto.Review;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IProductForSkinTypeService
    {
        Task<PagedResponse<ProductForSkinTypeDto>> GetProductsBySkinTypeIdAsync(Guid productId, int pageNumber, int pageSize);
    }
}
