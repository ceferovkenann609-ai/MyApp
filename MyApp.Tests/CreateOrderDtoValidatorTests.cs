using FluentAssertions;
using MyApp.Application.DTOs;
using MyApp.Application.Validators;
using Xunit;

namespace MyApp.Tests;

public class CreateOrderDtoValidatorTests
{
    private readonly CreateOrderDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenItemsIsEmpty()
    {
        var dto = new CreateOrderDto { Items = new List<CreateOrderItemDto>() };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuantityIsZeroOrNegative()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 0 }
            }
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenProductIdIsInvalid()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 0, Quantity = 1 }
            }
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 2 }
            }
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
