using AutoMapper;
using ProductManagement.Repository.Models.Domain;
using ProductManagement.Services.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductManagement.Services.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // CreateProductDTO → Product (ignores server-set fields)
        CreateMap<CreateProductDTO, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

        // UpdateProductDTO → Product (ignores identity and audit fields)
        CreateMap<UpdateProductDTO, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

        // Product → ProductDTO (direct mapping, all fields)
        CreateMap<Product, ProductDTO>();
    }
}