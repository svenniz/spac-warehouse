using Microsoft.EntityFrameworkCore;
using WarehouseApi.Models;

namespace WarehouseApi.Data_Access
{
    public class WarehouseContext : DbContext, IWarehouseContext
    {
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductAttributeKey> ProductAttributeKeys { get; set; } = null!;
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; } = null!;
        public DbSet<ProductAttributeMapping> ProductAttributeMappings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.ProductNumber).IsUnique();
            });

            // Configure ProductAttributeKey
            modelBuilder.Entity<ProductAttributeKey>(entity =>
            {
                entity.HasKey(pak => pak.Id);
            });

            // Configure ProductAttributeValue
            modelBuilder.Entity<ProductAttributeValue>(entity =>
            {
                entity.HasKey(pav => pav.Id);
            });

            // Configure ProductAttributeMapping
            modelBuilder.Entity<ProductAttributeMapping>(entity =>
            {
                entity.HasKey(pam => new { pam.ProductId, pam.ProductAttributeKeyId, pam.ProductAttributeValueId });

                entity.HasOne(pam => pam.Product)
                      .WithMany(p => p.ProductAttributes)
                      .HasForeignKey(pam => pam.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pam => pam.ProductAttributeKey)
                      .WithMany(pak => pak.ProductAttributeMappings)
                      .HasForeignKey(pam => pam.ProductAttributeKeyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pam => pam.ProductAttributeValue)
                      .WithMany(pav => pav.ProductAttributeMappings)
                      .HasForeignKey(pam => pam.ProductAttributeValueId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public WarehouseContext(DbContextOptions<WarehouseContext> options)
            : base(options) { }

        // SaveChangesAsync and SaveChanges
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
