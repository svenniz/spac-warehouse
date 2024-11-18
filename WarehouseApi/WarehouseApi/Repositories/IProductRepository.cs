using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        public async Task<IEnumerable<Product>> GetAllProducts();
    }
}