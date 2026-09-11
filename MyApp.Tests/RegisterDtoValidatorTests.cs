using FluentAssertions;
using MyApp.Application.DTOs;
using MyApp.Application.Validators;
using Xunit;

namespace MyApp.Tests;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsInvalid()
    {
        var dto = new RegisterDto { Name = "Test", Email = "not-an-email", Password = "123456" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPasswordTooShort()
    {
        var dto = new RegisterDto { Name = "Test", Email = "test@test.com", Password = "123" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new RegisterDto { Name = "Test", Email = "test@test.com", Password = "123456" };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
