using MyApp.Domain.Entities;

namespace MyApp.Application.Common;

public static class CouponDiscountCalculator
{
    public record Result(bool IsValid, string? ErrorMessage, decimal DiscountAmount);

    public static Result Calculate(Coupon coupon, decimal orderTotal, DateTime now)
    {
        if (!coupon.IsActive)
            return new Result(false, "Kupon aktiv deyil.", 0);

        if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < now)
            return new Result(false, "Kuponun müddəti bitib.", 0);

        if (coupon.MaxUsageCount.HasValue && coupon.UsedCount >= coupon.MaxUsageCount.Value)
            return new Result(false, "Kuponun istifadə limiti dolub.", 0);

        if (orderTotal < coupon.MinOrderAmount)
            return new Result(false, $"Minimum sifariş məbləği {coupon.MinOrderAmount} olmalıdır.", 0);

        var discount = coupon.DiscountType == DiscountType.Percentage
            ? Math.Round(orderTotal * (coupon.DiscountValue / 100), 2)
            : coupon.DiscountValue;

        if (discount > orderTotal)
            discount = orderTotal;

        return new Result(true, null, discount);
    }
}
