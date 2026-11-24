using DotnetStore.Api.Options;
using DotnetStore.Api.Payments;
using DotnetStore.Api.Services;
using StajDb.Models;

namespace DotnetStore.Tests;

public class MockPaymentProviderTests
{
    private static ChargeRequest Req(string number, int month = 12, int year = 2099, string cvv = "123") =>
        new(100m, "TRY", "T-1", new CardInfo("TEST", number, month, year, cvv));

    private readonly MockPaymentProvider _p = new();

    [Fact]
    public async Task Valid_test_card_succeeds_and_exposes_only_last4()
    {
        var r = await _p.ChargeAsync(Req("4242 4242 4242 4242"), default);
        Assert.True(r.Success);
        Assert.Equal("4242", r.CardLast4);
        Assert.Equal("Visa", r.CardBrand);
        Assert.StartsWith("MOCK-", r.TransactionId);
    }

    [Theory]
    [InlineData("4000000000000002")]
    [InlineData("4000000000009995")]
    public async Task Decline_cards_fail(string number)
    {
        var r = await _p.ChargeAsync(Req(number), default);
        Assert.False(r.Success);
        Assert.Null(r.TransactionId);
    }

    [Fact]
    public async Task Luhn_invalid_number_is_rejected() =>
        Assert.False((await _p.ChargeAsync(Req("4242424242424241"), default)).Success);

    [Fact]
    public async Task Expired_card_is_rejected() =>
        Assert.False((await _p.ChargeAsync(Req("4242424242424242", 1, 2020), default)).Success);

    [Theory]
    [InlineData("12")]
    [InlineData("abcd")]
    public async Task Bad_cvv_is_rejected(string cvv) =>
        Assert.False((await _p.ChargeAsync(Req("4242424242424242", cvv: cvv), default)).Success);
}

public class OrderRulesTests
{
    [Theory]
    [InlineData(OrderStatus.SiparisAlindi, OrderStatus.KargoyaVerildi, true)]
    [InlineData(OrderStatus.SiparisAlindi, OrderStatus.IptalEdildi, true)]
    [InlineData(OrderStatus.SiparisAlindi, OrderStatus.TeslimEdildi, false)]
    [InlineData(OrderStatus.KargoyaVerildi, OrderStatus.TeslimEdildi, true)]
    [InlineData(OrderStatus.TeslimEdildi, OrderStatus.IptalEdildi, false)]
    [InlineData(OrderStatus.IptalEdildi, OrderStatus.SiparisAlindi, false)]
    public void Transitions_follow_rules(OrderStatus from, OrderStatus to, bool expected) =>
        Assert.Equal(expected, OrderStatusHelper.IsAllowedTransition(from, to));

    [Theory]
    [InlineData(OrderStatus.SiparisAlindi, true)]
    [InlineData(OrderStatus.KargoyaVerildi, true)]
    [InlineData(OrderStatus.TeslimEdildi, true)]
    [InlineData(OrderStatus.IptalEdildi, false)]
    [InlineData(OrderStatus.Sepette, false)]
    public void Only_active_orders_hold_stock(OrderStatus s, bool expected) =>
        Assert.Equal(expected, OrderStatusHelper.ConsumesStock(s));

    [Theory]
    [InlineData("100", "49.9")]
    [InlineData("499.99", "49.9")]
    [InlineData("500", "0")]
    [InlineData("0", "0")]
    public void Shipping_is_free_over_threshold(string subtotal, string expected)
    {
        var o = new StoreOptions { FreeShippingThreshold = 500m, ShippingFee = 49.9m };
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        Assert.Equal(decimal.Parse(expected, inv), o.CalculateShipping(decimal.Parse(subtotal, inv)));
    }
}
