using AutoMapper;
using BusinessObjects.Dto.Product;
using BusinessObjects.Models;

namespace API.Extensions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Product
        CreateMap<Product, ProductDto>();
        CreateMap<ProductForCreationDto, Product>();
        CreateMap<ProductForUpdateDto, Product>();
        #endregion
    }
}