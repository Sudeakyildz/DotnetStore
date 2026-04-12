namespace StajDb.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? Slug { get; set; }

    public bool IsDeleted { get; set; }

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
