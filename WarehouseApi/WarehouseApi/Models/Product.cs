namespace WarehouseApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int StockQuantity { get; set; }

        public ICollection<ProductAttributeMapping> ProductAttributes { get; set; } = new List<ProductAttributeMapping>();
    }
}