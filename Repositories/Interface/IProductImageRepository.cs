using BusinessObjects.Models;
using Services.Response;

namespace Repositories.Interface
{
    public interface IProductImageRepository : IRepositoryBase<ProductImage, Guid>
    {
        Task<List<ProductImageByIdResponse>> GetImagesByProductIdAsync(Guid productId);
    }
}
