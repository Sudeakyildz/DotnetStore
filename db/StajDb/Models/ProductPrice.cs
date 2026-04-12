namespace StajDb.Models;

public class ProductPrice
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal Price { get; set; }

    public bool IsDiscount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; }
}
