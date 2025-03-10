using Microsoft.AspNetCore.Http;
using Services.Response;

namespace Services.Interface;

public interface IProductImageService
{
    Task<bool> UploadProductImage(List<IFormFile> files, Guid productId);

    Task<bool> DeleteProductImage(Guid ImageId);

    Task<IList<ProductImageByIdResponse>> GetProductImageById(Guid id);
}