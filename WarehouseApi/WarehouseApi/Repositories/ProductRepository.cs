using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    /// <summary>
    /// Non-Generic Repository for Products
    /// Performs persistence layer operations
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly WarehouseContext _warehouseContext;

        public ProductRepository(WarehouseContext warehouseContext)
        {
            _warehouseContext = warehouseContext;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _warehouseContext.Products.ToListAsync();
        }

        public async Task<Product> GetProduct(int productId)
        {
            var product = await _warehouseContext.Products.FindAsync(productId);
            return product;
        }

        public async Task<Product> AddProduct(Product product)
        {
            _warehouseContext.Products.Add(product);
            await _warehouseContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            _warehouseContext.Entry(product).State = EntityState.Modified;
            await _warehouseContext.SaveChangesAsync();
            return product;
        }

        public async void DeleteProduct(int productId)
        {
            var product = await _warehouseContext.Products.FindAsync(productId);

            if (product != null)
            {
                _warehouseContext.Products.Remove(product);
                await _warehouseContext.SaveChangesAsync();
            }
        }
    }
}
