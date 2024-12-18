﻿using WarehouseApi.Dto;
using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    /// <summary>
    /// Non-generic interface for product specific DAL operations
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<ProductDto> AddProductFromDto(ProductDto productDto);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductAsync(int id);
        Task<ProductDto?> GetProductDto(int id);
        IQueryable<Product> GetProductWithIncludes();
        bool ProductExists(int id);
        void UpdateProductAsync(Product product);
        Task<ProductDto> UpdateProductFromDto(int id, ProductDto productdto);
    }
}