using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data_Access;
using WarehouseApi.Models;

namespace WarehouseApi.Repositories
{
    public class GenericProductRepository : IRepository<Product>
    {
        private readonly WarehouseContext _warehouseContext;
        private readonly DbSet<T> _dbSet;

        public GenericProductRepository(WarehouseContext warehouseContext, )
        {
            _warehouseContext = warehouseContext;
        }
        public void Add(Product entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> Get(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task SaveChanges()
        {
            throw new NotImplementedException();
        }

        public void Update(Product entity)
        {
            throw new NotImplementedException();
        }
    }
}
