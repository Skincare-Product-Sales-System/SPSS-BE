using Microsoft.AspNetCore.Http;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implementation
{
    public class ImageService : IImageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<string>> MigrateToFirebaseLinkList(List<IFormFile> files)
        {
            var uploadImageService = new ManageFirebaseImage.ManageFirebaseImageService();
            List<string> downloadUrl = [];
            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    throw new Exception("File is empty");
                }

                using (var stream = file.OpenReadStream())
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var imageUrl = await uploadImageService.UploadFileAsync(stream, fileName);
                    downloadUrl.Add(imageUrl);
                }
            }
            return downloadUrl;

        }
        public async Task<bool> DeleteFirebaseLink(string imageUrl)
        {

            var deleteImageService = new ManageFirebaseImage.ManageFirebaseImageService();
            await deleteImageService.DeleteFileAsync(imageUrl);

            return true;
        }
    }
}
