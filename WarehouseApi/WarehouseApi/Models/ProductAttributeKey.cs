using System;

namespace WarehouseApi.Models;

public class ProductAttributeKey
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<ProductAttributeMapping> ProductAttributeMappings { get; set; } = new List<ProductAttributeMapping>();
}

