using Microsoft.EntityFrameworkCore;
using WarehouseApi.Models;

namespace WarehouseApi.Data_Access
{
    public interface IWarehouseContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<ProductAttributeMapping> ProductAttributeMappings { get; set; }
        DbSet<T> Set<T>() where T : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
