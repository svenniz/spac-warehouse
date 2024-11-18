using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Dto;
using WarehouseApi.Models;

namespace WarehouseApi.Factories
{
    public class ProductFactory
    {
        private readonly WarehouseContext _context;

        public ProductFactory(WarehouseContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateProductAsync(ProductDto productDto)
        {
            var product = new Product
            {
                ProductNumber = productDto.ProductNumber,
                Name = productDto.Name,
                Description = productDto.Description,
                StockQuantity = productDto.StockQuantity,
                ProductAttributes = new List<ProductAttributeMapping>()
            };

            foreach (var attr in productDto.ProductAttributes)
            {
                var keyName = attr.Keys.First();
                var valueName = attr.Values.First();

                var key = await _context.ProductAttributeKeys.FirstOrDefaultAsync(k => k.Name == keyName);
                if (key == null)
                {
                    key = new ProductAttributeKey { Name = keyName };
                    _context.ProductAttributeKeys.Add(key);
                }

                var value = await _context.ProductAttributeValues.FirstOrDefaultAsync(v => v.Value == valueName);
                if (value == null)
                {
                    value = new ProductAttributeValue { Value = valueName, Type = "string" };
                    _context.ProductAttributeValues.Add(value);
                }

                product.ProductAttributes.Add(new ProductAttributeMapping
                {
                    ProductAttributeKey = key,
                    ProductAttributeValue = value,
                });
            }
            return product;
        }

        public ProductDto CreateProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                ProductNumber = product.ProductNumber,
                Name = product.Name,
                Description = product.Description,
                StockQuantity = product.StockQuantity,
                ProductAttributes = product.ProductAttributes.Select(attr => new Dictionary<string, string>
                {
                    { "Key", attr.ProductAttributeKey.Name },
                    { "Value", attr.ProductAttributeValue.Value }
                }).ToList()
            };
        }
    }
}