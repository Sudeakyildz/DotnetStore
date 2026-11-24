namespace StajDb.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerUserId { get; set; }
    public StoreUser? Customer { get; set; }

    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    // Teslimat bilgisi (mağaza siparişlerinde dolu)
    public string? ShippingFullName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingAddress { get; set; }

    public decimal ShippingFee { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TrackingNumber { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
