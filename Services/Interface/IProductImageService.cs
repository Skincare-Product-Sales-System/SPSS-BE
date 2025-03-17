using Microsoft.AspNetCore.Http;
using Services.Response;

namespace Services.Interface;

public interface IProductImageService
{
    Task<IList<string>> MigrateToFirebaseLinkList(List<IFormFile> files);
    Task<bool> UploadProductImage(List<IFormFile> files, Guid productId);

    Task<bool> DeleteProductImage(Guid ImageId);

    Task<IList<ProductImageByIdResponse>> GetProductImageById(Guid id);

    Task<bool> DeleteFirebaseLink(string imageUrl);
}