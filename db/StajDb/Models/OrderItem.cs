namespace StajDb.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// Satır anındaki birim fiyat (ürün fiyatı kopyası).
    /// </summary>
    public decimal UnitPrice { get; set; }
}
