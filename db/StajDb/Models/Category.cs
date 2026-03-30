namespace StajDb.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    // Admin ekranda kategori görseli için kullanılabilir.
    public string? ImageUrl { get; set; }

    // Menü/route için slug (opsiyonel ama pratik).
    public string? Slug { get; set; }

    // Staj: soft delete
    public bool IsDeleted { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

