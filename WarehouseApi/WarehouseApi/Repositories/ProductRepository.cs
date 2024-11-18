using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
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
        public ProductRepository(WarehouseContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Get All Products and return list where attribute key and value are included.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            var products = await _context.Products.Include(p => p.ProductAttributes)
                .ThenInclude(pa => pa.ProductAttributeKey)
                .Include(p => p.ProductAttributes)
                .ThenInclude(pa => pa.ProductAttributeValue)
                .ToListAsync();

            //List<ProductDto> productDtos = products.Select(p => _productFactory.CreateProductDto(p)).ToList();

            return products;
        }
    }
}
