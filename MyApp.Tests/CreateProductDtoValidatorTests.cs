using FluentAssertions;
using MyApp.Application.DTOs;
using MyApp.Application.Validators;
using Xunit;

namespace MyApp.Tests;

public class CreateProductDtoValidatorTests
{
    private readonly CreateProductDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenPriceIsZeroOrNegative()
    {
        var dto = new CreateProductDto { Name = "Test", Price = 0, Stock = 5, CategoryId = 1 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenStockIsNegative()
    {
        var dto = new CreateProductDto { Name = "Test", Price = 10, Stock = -1, CategoryId = 1 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        var dto = new CreateProductDto { Name = "", Price = 10, Stock = 5, CategoryId = 1 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new CreateProductDto { Name = "iPhone", Price = 1200, Stock = 10, CategoryId = 1 };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
