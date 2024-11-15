using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> Add(Product product);
        void Delete(Product product);
        Task<Product> Get(int id);
        Task<IEnumerable<Product>> GetAll();
        Task<Product> Update(Product product);
        Task SaveChanges();
    }
}