namespace StajDb.Models;

public class Feature
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    // Basitçe string olarak tutuyoruz; ileride datatype üzerinden validasyon yapılabilir.
    public string? DataType { get; set; }

    public bool IsDeleted { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductFeatureValue> Values { get; set; } = new List<ProductFeatureValue>();
}

