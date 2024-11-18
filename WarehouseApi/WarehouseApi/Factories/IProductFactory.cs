using WarehouseApi.Dto;
using WarehouseApi.Models;

namespace WarehouseApi.Factories
{
    public interface IProductFactory
    {
        Task<Product> CreateProductAsync(ProductDto productDto);
        ProductDto CreateProductDto(Product product);
    }
}