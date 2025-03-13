using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System;
using System.Threading.Tasks;
using Services.Response;
using BusinessObjects.Dto.ProductForSkinType;
using Services.Dto.Api;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products-for-skin-type")]
    public class ProductForSkinTypeController : ControllerBase
    {
        private readonly IProductForSkinTypeService _productForSkinTypeService;
        public ProductForSkinTypeController(IProductForSkinTypeService productForSkinTypeService) => _productForSkinTypeService = productForSkinTypeService ?? throw new ArgumentNullException(nameof(productForSkinTypeService));

        [HttpGet("{skinTypeId:guid}")]
        public async Task<IActionResult> GetProductsBySkinTypeId(
            Guid skinTypeId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<object>.FailureResponse("Số trang và số lượng sản phẩm phải lớn hơn 0."));
            }

            var result = await _productForSkinTypeService.GetProductsBySkinTypeIdAsync(skinTypeId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResponse<ProductForSkinTypeDto>>.SuccessResponse(result));
        }
    }
}
