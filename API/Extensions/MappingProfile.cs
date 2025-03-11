using AutoMapper;
using BusinessObjects.Dto.Address;
using BusinessObjects.Dto.CancelReason;
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

        #region CancelReason
        CreateMap<CancelReason, CancelReasonDto>();
        CreateMap<CancelReasonForCreationDto, CancelReason>();
        CreateMap<CancelReasonForUpdateDto, CancelReason>();
        #endregion

        #region Address
        CreateMap<Address, AddressDto>();
        CreateMap<AddressForCreationDto, Address>();
        CreateMap<AddressForUpdateDto, Address>();
        #endregion
    }
}