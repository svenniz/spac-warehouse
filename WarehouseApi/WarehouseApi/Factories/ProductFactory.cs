using WarehouseApi.Dto;
using WarehouseApi.Models;
using WarehouseApi.Repositories;

namespace WarehouseApi.Factories
{
    public class ProductFactory : IProductFactory
    {

        private readonly IRepository<ProductAttributeKey> _attributeKeyRepository;
        private readonly IRepository<ProductAttributeValue> _attributeValueRepository;

        public ProductFactory(IRepository<ProductAttributeKey> attributeKeyRepository, IRepository<ProductAttributeValue> attributeValueRepository)
        {
            _attributeKeyRepository = attributeKeyRepository;
            _attributeValueRepository = attributeValueRepository;
        }

        /// <summary>
        /// Creates a new <see cref="Product"/> object from the specified <see cref="ProductDto"/>.
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns></returns>
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
                var keyName = attr.Key;
                var valueName = attr.Value;

                var key = await _attributeKeyRepository.GetOrCreateBySelector(k => k.Name == keyName, () => new ProductAttributeKey { Name = keyName });
                var value = await _attributeValueRepository.GetOrCreateBySelector(v => v.Value == valueName, () => new ProductAttributeValue { Value = valueName, Type = "string" });

                product.ProductAttributes.Add(new ProductAttributeMapping
                {
                    ProductAttributeKey = key,
                    ProductAttributeValue = value,
                });
            }
            return product;
        }

        /// <summary>
        /// Creates a new <see cref="ProductDto"/> object from the specified <see cref="Product"/>.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public ProductDto CreateProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                ProductNumber = product.ProductNumber,
                Name = product.Name,
                Description = product.Description,
                StockQuantity = product.StockQuantity,
                ProductAttributes = product.ProductAttributes.Select(attr => new ProductAttributeDto
                {
                    Key = attr.ProductAttributeKey.Name,
                    Value = attr.ProductAttributeValue.Value
                }).ToList()
            };
        }
    }
}