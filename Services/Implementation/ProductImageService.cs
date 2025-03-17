using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;


    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }

        public async Task<bool> UploadProductImage(List<IFormFile> files, Guid productId)
        {
            if (files == null || files.Count == 0)
            {
                throw new Exception("File not found");
            }
            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw new Exception("Product not found");

            var uploadImageService = new ManageFirebaseImage.ManageFirebaseImageService();

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
                    
                    var productImage = new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = imageUrl,
                        ProductId = productId
                    };

                    _unitOfWork.ProductImages.Add(productImage);
                }
            }

            await _unitOfWork.SaveChangesAsync(); 

            return true;
        }

        public async Task<bool> DeleteProductImage(Guid imageId)
        {
            var productImage = await _unitOfWork.ProductImages.GetByIdAsync(imageId)
                ?? throw new Exception("File not found");

            var deleteImageService = new ManageFirebaseImage.ManageFirebaseImageService();
            await deleteImageService.DeleteFileAsync(productImage.ImageUrl);

            _unitOfWork.ProductImages.Delete(productImage);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IList<ProductImageByIdResponse>> GetProductImageById(Guid id)
        {
            var images = await _unitOfWork.ProductImages.GetImagesByProductIdAsync(id);


            return images;
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
