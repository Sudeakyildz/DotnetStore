namespace StajDb.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerUserId { get; set; }
    public StoreUser? Customer { get; set; }

    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
