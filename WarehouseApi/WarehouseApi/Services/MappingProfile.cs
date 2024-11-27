using AutoMapper;
using System.Runtime.InteropServices;
using WarehouseApi.Models;
using WarehouseApi.Dto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductAttributeMapping, ProductAttributeDto>()
            .ForMember(dest => dest.Key,
            opt=>opt.MapFrom(src=>src.ProductAttributeKey.Name))
            .ForMember(dest=> dest.Value,
            opt=>opt.MapFrom(src=>src.ProductAttributeValue.Value));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ProductAttributes,
            opt => opt.MapFrom(src => src.ProductAttributes));
    }
}