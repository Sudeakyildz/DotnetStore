namespace DotnetStore.Api.Payments;

public sealed record CardInfo(string HolderName, string Number, int ExpMonth, int ExpYear, string Cvv);

public sealed record ChargeRequest(decimal Amount, string Currency, string Reference, CardInfo Card);

public sealed record ChargeResult(
    bool Success,
    string? TransactionId,
    string? CardBrand,
    string? CardLast4,
    string? Error);

public sealed record RefundResult(bool Success, string? TransactionId, string? Error);

/// <summary>
/// Ödeme sağlayıcı soyutlaması. Gerçek ortamda iyzico / PayTR / Stripe gibi bir sağlayıcı için
/// bu arayüzü uygulayın ve Program.cs'te kaydı değiştirin. Kart verisi uygulamada saklanmaz.
/// </summary>
public interface IPaymentProvider
{
    string Name { get; }
    Task<ChargeResult> ChargeAsync(ChargeRequest request, CancellationToken ct);
    Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken ct);
}
