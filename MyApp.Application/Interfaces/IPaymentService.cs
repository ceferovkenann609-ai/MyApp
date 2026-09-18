namespace MyApp.Application.Interfaces;

public record ChargeResult(bool Success, string TransactionReference, string? FailureReason);

public interface IPaymentService
{
    Task<ChargeResult> ChargeAsync(decimal amount, CancellationToken ct = default);
}
