using FluentValidation;
using MyApp.Application.DTOs;

namespace MyApp.Application.Validators;

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountType).NotEmpty().Must(x => x == "Percentage" || x == "FixedAmount")
            .WithMessage("DiscountType 'Percentage' və ya 'FixedAmount' olmalıdır.");
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0);
    }
}

public class ApplyCouponDtoValidator : AbstractValidator<ApplyCouponDto>
{
    public ApplyCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
    }
}
