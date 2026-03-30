namespace StajDb.Models;

public class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Stock { get; set; }

    // Staj: active | inactive | draft
    public string Status { get; set; } = "active";

    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation (EAV / fiyat geçmişi)
    public ICollection<ProductFeatureValue> FeatureValues { get; set; } = new List<ProductFeatureValue>();
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
}

