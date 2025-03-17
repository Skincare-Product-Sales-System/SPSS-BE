using Microsoft.AspNetCore.Http;
using Services.Implementation;

namespace Services.Interface
{
    public interface IImageService
    {
        Task<IList<string>> MigrateToFirebaseLinkList(List<IFormFile> files);
        Task<bool> DeleteFirebaseLink(string imageUrl);
    }
}
