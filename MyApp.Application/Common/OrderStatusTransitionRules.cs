using MyApp.Domain.Entities;

namespace MyApp.Application.Common;

public static class OrderStatusTransitionRules
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled, OrderStatus.Shipped },
        [OrderStatus.Shipped] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

    public static bool IsValid(OrderStatus current, OrderStatus next)
    {
        return Allowed.TryGetValue(current, out var allowedNext) && allowedNext.Contains(next);
    }
}
