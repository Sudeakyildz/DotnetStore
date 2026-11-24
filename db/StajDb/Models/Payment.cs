namespace StajDb.Models;

/// <summary>Bir siparişe ait ödeme / iade hareketi. Kart numarası veya CVV ASLA saklanmaz.</summary>
public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public string Provider { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }

    public string? TransactionId { get; set; }
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }
}
