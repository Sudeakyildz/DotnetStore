namespace StajDb.Models;

public class ProductPrice
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal Price { get; set; }

    // Staj: referans fiyat/indirim mantığı
    public bool IsDiscount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

