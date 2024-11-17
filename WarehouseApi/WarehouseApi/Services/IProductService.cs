using WarehouseApi.Models;

namespace WarehouseApi.Services
{
    public interface IProductService
    {
        public Task<IEnumerable<Product>> GetProductByKeyValuesAsync(Dictionary<string,string> query);
    }
}
