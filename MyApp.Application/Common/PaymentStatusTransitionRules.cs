using MyApp.Domain.Entities;

namespace MyApp.Application.Common;

public static class PaymentStatusTransitionRules
{
    private static readonly Dictionary<PaymentStatus, PaymentStatus[]> Allowed = new()
    {
        [PaymentStatus.Pending] = new[] { PaymentStatus.Completed, PaymentStatus.Failed },
        [PaymentStatus.Completed] = new[] { PaymentStatus.Refunded },
        [PaymentStatus.Failed] = Array.Empty<PaymentStatus>(),
        [PaymentStatus.Refunded] = Array.Empty<PaymentStatus>()
    };

    public static bool IsValid(PaymentStatus current, PaymentStatus next)
    {
        return Allowed.TryGetValue(current, out var allowedNext) && allowedNext.Contains(next);
    }
}
