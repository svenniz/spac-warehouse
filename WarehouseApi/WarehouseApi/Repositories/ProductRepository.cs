﻿using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Dto;
using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    /// <summary>
    /// Implementation of Non-Generic Repository for Products
    /// Performs persistence layer operations on Products. Only methods not implemented in GenericEfCoreRepository is implemented here.
    /// </summary>
    public class ProductRepository : GenericEfCoreRepository<Product>, IProductRepository
    {
        private readonly WarehouseContext _context;
        private readonly IMapper _mapper;
        public ProductRepository(WarehouseContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Get Product with Attribute key and value
        /// </summary>
        /// <returns></returns>
        public async Task<Product?> GetProductAsync(int id)
        {
            return await GetProductWithIncludes()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Get product by id and return a ProductDto using the MappingProfile and Automapper
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ProductDto?> GetProductDto(int id)
        {
            var product = await GetProductWithIncludes()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return null;
            }
            return _mapper.Map<ProductDto>(product);
        }
        /// <summary>
        /// Get All Products and return list where attribute key and value are included.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await GetProductWithIncludes().ToListAsync();
        }

        /// <summary>
        /// Returns IQueryable to use for Product-specific operations. Includes Attribute Key and Attribute Value.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Product> GetProductWithIncludes()
        {
            return _context.Products
                .Include(p => p.ProductAttributes)
                .ThenInclude(pa => pa.ProductAttributeKey)
                .Include(p => p.ProductAttributes)
                .ThenInclude(pa => pa.ProductAttributeValue);
        }

        /// <summary>
        /// Add product using mapper
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns></returns>
        public async Task<ProductDto?> AddProductFromDto(ProductDto productDto)
        {
            if (productDto == null)
            {
                return null;
            }
            var product = _mapper.Map<Product>(productDto);
            Add(product);

            return _mapper.Map<ProductDto>(product);
        }

        /// <summary>
        /// Update using mapper
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productdto"></param>
        /// <returns></returns>
        public async Task<ProductDto?> UpdateProductFromDto(int id, ProductDto productdto)
        {
            var existingProduct = await GetProductWithIncludes()
                                        .FirstOrDefaultAsync(p => p.Id == id);
            if (existingProduct == null) 
            {
                return null;
            }
            _mapper.Map(productdto, existingProduct);

            return _mapper.Map<ProductDto?>(existingProduct);
        }

        /// <summary>
        /// Updates product and marks it as dirty
        /// </summary>
        /// <param name="product"></param>
        public void UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
        }
        /// <summary>
        /// Checks if there are any products with Id
        /// </summary>
        /// <param name="id">Product Id</param>
        /// <returns></returns>
        public bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
