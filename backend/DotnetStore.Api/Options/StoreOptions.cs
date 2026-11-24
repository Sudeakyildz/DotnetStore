namespace DotnetStore.Api.Options;

public sealed class StoreOptions
{
    public const string SectionName = "Store";

    /// <summary>Bu tutar ve üzeri siparişlerde kargo ücretsiz.</summary>
    public decimal FreeShippingThreshold { get; set; } = 500m;
    public decimal ShippingFee { get; set; } = 49.9m;

    public decimal CalculateShipping(decimal subtotal) =>
        subtotal <= 0m || subtotal >= FreeShippingThreshold ? 0m : ShippingFee;
}
