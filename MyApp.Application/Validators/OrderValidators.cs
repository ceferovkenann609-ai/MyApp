using FluentValidation;
using MyApp.Application.DTOs;

namespace MyApp.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("Sifarişdə ən azı bir məhsul olmalıdır.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity müsbət olmalıdır.");
        });
    }
}

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty();
    }
}
