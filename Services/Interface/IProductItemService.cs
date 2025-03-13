using BusinessObjects.Dto.ProductItem;
using Microsoft.AspNetCore.Http;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IProductItemService
    {
        Task<bool> UploadProductItemImage(List<IFormFile> files, Guid Id);

    }
}
