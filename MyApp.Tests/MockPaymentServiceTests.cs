using FluentAssertions;
using MyApp.Infrastructure.Services;
using Xunit;

namespace MyApp.Tests;

public class MockPaymentServiceTests
{
    private readonly MockPaymentService _service = new();

    [Fact]
    public async Task ChargeAsync_ShouldSucceed_WhenAmountIsPositive()
    {
        var result = await _service.ChargeAsync(100);

        result.Success.Should().BeTrue();
        result.TransactionReference.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChargeAsync_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        var result = await _service.ChargeAsync(0);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().NotBeNullOrEmpty();
    }
}
