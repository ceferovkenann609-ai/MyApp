using MyApp.Application.Interfaces;

namespace MyApp.Infrastructure.Services;

public class MockPaymentService : IPaymentService
{
    public Task<ChargeResult> ChargeAsync(decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0)
            return Task.FromResult(new ChargeResult(false, string.Empty, "Ödəniş məbləği düzgün deyil."));

        var reference = $"MOCK-{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new ChargeResult(true, reference, null));
    }
}
