namespace StajDb.Models;

public class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public StoreUser? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
