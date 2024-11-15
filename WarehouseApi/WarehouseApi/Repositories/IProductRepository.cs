using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    public interface IProductRepository
    {
        Task<Product> AddProduct(Product product);
        void DeleteProduct(int productId);
        Task<Product> GetProduct(int productId);
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> UpdateProduct(Product product);
    }
}