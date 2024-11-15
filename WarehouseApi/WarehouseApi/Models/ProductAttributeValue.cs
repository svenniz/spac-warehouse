using System;

namespace WarehouseApi.Models;

public class ProductAttributeValue
{
    public int Id { get; set; }
    public string Value { get; set; } = null!;
    public string Type { get; set; } = null!;

    public ICollection<ProductAttributeMapping> ProductAttributeMappings { get; set; } = new List<ProductAttributeMapping>();
}

