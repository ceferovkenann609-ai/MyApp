using FluentAssertions;
using MyApp.Application.Common;
using MyApp.Domain.Entities;
using Xunit;

namespace MyApp.Tests;

public class PaymentStatusTransitionRulesTests
{
    [Theory]
    [InlineData(PaymentStatus.Pending, PaymentStatus.Completed, true)]
    [InlineData(PaymentStatus.Pending, PaymentStatus.Failed, true)]
    [InlineData(PaymentStatus.Completed, PaymentStatus.Refunded, true)]
    public void IsValid_ShouldReturnTrue_ForAllowedTransitions(PaymentStatus from, PaymentStatus to, bool expected)
    {
        PaymentStatusTransitionRules.IsValid(from, to).Should().Be(expected);
    }

    [Theory]
    [InlineData(PaymentStatus.Failed, PaymentStatus.Completed)]
    [InlineData(PaymentStatus.Refunded, PaymentStatus.Completed)]
    [InlineData(PaymentStatus.Completed, PaymentStatus.Pending)]
    public void IsValid_ShouldReturnFalse_ForDisallowedTransitions(PaymentStatus from, PaymentStatus to)
    {
        PaymentStatusTransitionRules.IsValid(from, to).Should().BeFalse();
    }
}
