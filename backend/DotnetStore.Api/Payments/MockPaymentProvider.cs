namespace DotnetStore.Api.Payments;

/// <summary>
/// Geliştirme/demo için sahte sağlayıcı: gerçek para hareketi YOKTUR.
/// Test kartları: 4242 4242 4242 4242 (başarılı), 4000 0000 0000 0002 (reddedildi),
/// 4000 0000 0000 9995 (yetersiz bakiye). Diğer geçerli (Luhn) kartlar başarılı sayılır.
/// </summary>
public sealed class MockPaymentProvider : IPaymentProvider
{
    public string Name => "Mock";

    public Task<ChargeResult> ChargeAsync(ChargeRequest request, CancellationToken ct)
    {
        var digits = new string(request.Card.Number.Where(char.IsDigit).ToArray());
        var last4 = digits.Length >= 4 ? digits[^4..] : digits;
        var brand = DetectBrand(digits);

        if (digits.Length < 13 || digits.Length > 19 || !PassesLuhn(digits))
            return Task.FromResult(new ChargeResult(false, null, brand, last4, "Kart numarası geçersiz."));

        var cvvOk = request.Card.Cvv.Length is 3 or 4 && request.Card.Cvv.All(char.IsDigit);
        if (!cvvOk)
            return Task.FromResult(new ChargeResult(false, null, brand, last4, "CVV geçersiz."));

        var year = request.Card.ExpYear < 100 ? 2000 + request.Card.ExpYear : request.Card.ExpYear;
        if (request.Card.ExpMonth is < 1 or > 12)
            return Task.FromResult(new ChargeResult(false, null, brand, last4, "Son kullanma ayı geçersiz."));
        var now = DateTime.UtcNow;
        if (year < now.Year || (year == now.Year && request.Card.ExpMonth < now.Month))
            return Task.FromResult(new ChargeResult(false, null, brand, last4, "Kartın son kullanma tarihi geçmiş."));

        if (digits == "4000000000000002")
            return Task.FromResult(new ChargeResult(false, null, brand, last4, "Kart bankanız tarafından reddedildi."));
        if (digits == "4000000000009995")
            return Task.FromResult(new ChargeResult(false, null, brand, last4, "Kartta yeterli bakiye yok."));

        var tx = "MOCK-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
        return Task.FromResult(new ChargeResult(true, tx, brand, last4, null));
    }

    public Task<RefundResult> RefundAsync(string transactionId, decimal amount, CancellationToken ct) =>
        Task.FromResult(new RefundResult(true, "REFUND-" + transactionId, null));

    private static string DetectBrand(string d)
    {
        if (d.StartsWith('4')) return "Visa";
        if (d.Length >= 2 && int.TryParse(d[..2], out var p2) && (p2 is >= 51 and <= 55)) return "Mastercard";
        if (d.StartsWith("34") || d.StartsWith("37")) return "Amex";
        if (d.StartsWith("9792")) return "Troy";
        return "Kart";
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var alt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var n = digits[i] - '0';
            if (alt)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }

            sum += n;
            alt = !alt;
        }

        return sum % 10 == 0;
    }
}
