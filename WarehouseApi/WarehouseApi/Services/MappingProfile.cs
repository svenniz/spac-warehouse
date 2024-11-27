using AutoMapper;
using System.Runtime.InteropServices;
using WarehouseApi.Models;
using WarehouseApi.Dto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // From ProductAttributeMapping to ProductAttributeDto
        CreateMap<ProductAttributeMapping, ProductAttributeDto>()
            .ForMember(dest => dest.Key,
            opt=>opt.MapFrom(src=>src.ProductAttributeKey.Name))
            .ForMember(dest=> dest.Value,
            opt=>opt.MapFrom(src=>src.ProductAttributeValue.Value));

        // From ProductAttributeDto to ProductAttributeMapping. For POST/PUT
        CreateMap<ProductAttributeDto, ProductAttributeMapping>()
            .ForMember(dest => dest.ProductAttributeKey,
            opt => opt.MapFrom(src => new ProductAttributeKey { Name = src.Key }))
            .ForMember(dest => dest.ProductAttributeValue,
            opt => opt.MapFrom(src => new ProductAttributeValue { Value = src.Value }));


        // From Product Model to ProductDto
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ProductAttributes,
            opt => opt.MapFrom(src => src.ProductAttributes));

        // From ProductDto to Product Model
        CreateMap<ProductDto, Product>()
            .ForMember(dest=> dest.ProductAttributes,
            opt=>opt.MapFrom(src=>
            src.ProductAttributes.Select(dto=>new ProductAttributeMapping
            {
                ProductAttributeKey = new ProductAttributeKey { Name = dto.Key },
                ProductAttributeValue = new ProductAttributeValue { Value = dto.Value}
            }).ToList()));
    }
}