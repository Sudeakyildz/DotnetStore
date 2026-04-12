namespace StajDb.Models;

public class ProductFeatureValue
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int FeatureId { get; set; }
    public Feature? Feature { get; set; }

    public string Value { get; set; } = null!;

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; }
}
