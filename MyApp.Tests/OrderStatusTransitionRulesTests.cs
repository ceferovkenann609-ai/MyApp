using FluentAssertions;
using MyApp.Application.Common;
using MyApp.Domain.Entities;
using Xunit;

namespace MyApp.Tests;

public class OrderStatusTransitionRulesTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    public void IsValid_ShouldReturnTrue_ForAllowedTransitions(OrderStatus from, OrderStatus to, bool expected)
    {
        var result = OrderStatusTransitionRules.IsValid(from, to);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
    public void IsValid_ShouldReturnFalse_ForDisallowedTransitions(OrderStatus from, OrderStatus to)
    {
        var result = OrderStatusTransitionRules.IsValid(from, to);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenStatusIsTerminal()
    {
        OrderStatusTransitionRules.IsValid(OrderStatus.Delivered, OrderStatus.Confirmed).Should().BeFalse();
        OrderStatusTransitionRules.IsValid(OrderStatus.Cancelled, OrderStatus.Pending).Should().BeFalse();
    }
}
