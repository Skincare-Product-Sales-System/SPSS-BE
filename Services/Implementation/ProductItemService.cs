using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.CartItem;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Interface;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ProductItemService : IProductItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> UploadProductItemImage(List<IFormFile> files, Guid Id)
        {
            if (files == null || files.Count == 0)
            {
                throw new Exception("File not found");
            }
            var productItem = await _unitOfWork.ProductItems.GetByIdAsync(Id)
                ?? throw new Exception("Product item not found");

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

                    productItem.ImageUrl = imageUrl;

                    _unitOfWork.ProductItems.Update(productItem);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
