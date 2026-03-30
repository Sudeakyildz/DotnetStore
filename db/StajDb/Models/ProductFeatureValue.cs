namespace StajDb.Models;

public class ProductFeatureValue
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int FeatureId { get; set; }
    public Feature? Feature { get; set; }

    public string Value { get; set; } = null!;

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

