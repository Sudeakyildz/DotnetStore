namespace StajDb.Models;

public class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Stock { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductFeatureValue> FeatureValues { get; set; } = new List<ProductFeatureValue>();
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
}
