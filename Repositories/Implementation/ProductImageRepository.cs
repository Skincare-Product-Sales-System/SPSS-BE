using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Response;

namespace Repositories.Implementation;

public class ProductImageRepository : RepositoryBase<ProductImage, Guid>, IProductImageRepository
{
    public ProductImageRepository(SPSSContext context) : base(context)
    {
        
    }
    public async Task<List<ProductImageByIdResponse>> GetImagesByProductIdAsync(Guid productId)
    {
        return await _context.Set<ProductImage>()
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.CreatedTime)
            .Select(pi => new ProductImageByIdResponse
            {
                Id = pi.Id,
                Url = pi.ImageUrl,
            })
            .ToListAsync();
    }
}