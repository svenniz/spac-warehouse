using System;

namespace WarehouseApi.Dto;

public class ProductDto
{
    public int? Id { get; set; }
    public string ProductNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int StockQuantity { get; set; }

    public ICollection<Dictionary<string, string>> ProductAttributes { get; set; } = new List<Dictionary<string, string>>();
}
