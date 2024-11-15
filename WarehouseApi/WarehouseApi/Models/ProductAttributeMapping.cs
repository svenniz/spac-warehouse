using System;

namespace WarehouseApi.Models;

public class ProductAttributeMapping
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int ProductAttributeKeyId { get; set; }
    public ProductAttributeKey ProductAttributeKey { get; set; } = null!;

    public int ProductAttributeValueId { get; set; }
    public ProductAttributeValue ProductAttributeValue { get; set; } = null!;
}
