using Microsoft.EntityFrameworkCore;
using WarehouseApi.Models;

namespace WarehouseApi.Data_Access
{
    public class WarehouseContext : DbContext
    {
        public WarehouseContext(DbContextOptions<WarehouseContext> options) :base(options)
        {
            
        }
        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity => 
            {
                entity.HasKey(p => p.Id).HasName("PRIMARY");
                entity.ToTable("Products");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
