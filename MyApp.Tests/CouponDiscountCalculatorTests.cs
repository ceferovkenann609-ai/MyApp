using FluentAssertions;
using MyApp.Application.Common;
using MyApp.Domain.Entities;
using Xunit;

namespace MyApp.Tests;

public class CouponDiscountCalculatorTests
{
    private static Coupon MakeCoupon(
        DiscountType type = DiscountType.Percentage,
        decimal value = 10,
        decimal minOrder = 0,
        int? maxUsage = null,
        int used = 0,
        DateTime? expiry = null,
        bool isActive = true)
    {
        return new Coupon
        {
            Code = "TEST10",
            DiscountType = type,
            DiscountValue = value,
            MinOrderAmount = minOrder,
            MaxUsageCount = maxUsage,
            UsedCount = used,
            ExpiryDate = expiry,
            IsActive = isActive
        };
    }

    [Fact]
    public void Calculate_ShouldFail_WhenCouponIsInactive()
    {
        var coupon = MakeCoupon(isActive: false);

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeFalse();
        result.DiscountAmount.Should().Be(0);
    }

    [Fact]
    public void Calculate_ShouldFail_WhenExpired()
    {
        var coupon = MakeCoupon(expiry: DateTime.UtcNow.AddDays(-1));

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("müddəti");
    }

    [Fact]
    public void Calculate_ShouldFail_WhenUsageLimitReached()
    {
        var coupon = MakeCoupon(maxUsage: 5, used: 5);

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("limit");
    }

    [Fact]
    public void Calculate_ShouldFail_WhenOrderBelowMinimum()
    {
        var coupon = MakeCoupon(minOrder: 200);

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Calculate_ShouldApplyPercentageDiscount_Correctly()
    {
        var coupon = MakeCoupon(type: DiscountType.Percentage, value: 20);

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(20);
    }

    [Fact]
    public void Calculate_ShouldApplyFixedAmountDiscount_Correctly()
    {
        var coupon = MakeCoupon(type: DiscountType.FixedAmount, value: 30);

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(30);
    }

    [Fact]
    public void Calculate_ShouldCapDiscount_AtOrderTotal()
    {
        var coupon = MakeCoupon(type: DiscountType.FixedAmount, value: 500);

        var result = CouponDiscountCalculator.Calculate(coupon, 100, DateTime.UtcNow);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(100);
    }
}
