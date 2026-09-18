using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.DTOs;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CouponsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateCouponDto> _validator;

    public CouponsController(AppDbContext context, IValidator<CreateCouponDto> validator)
    {
        _context = context;
        _validator = validator;
    }

    private static CouponDto ToDto(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        DiscountType = c.DiscountType.ToString(),
        DiscountValue = c.DiscountValue,
        MinOrderAmount = c.MinOrderAmount,
        MaxUsageCount = c.MaxUsageCount,
        UsedCount = c.UsedCount,
        ExpiryDate = c.ExpiryDate,
        IsActive = c.IsActive
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CouponDto>>> GetAll()
    {
        var coupons = await _context.Coupons.ToListAsync();
        return Ok(coupons.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(CreateCouponDto dto)
    {
        await _validator.ValidateAndThrowAsync(dto);

        if (await _context.Coupons.AnyAsync(c => c.Code == dto.Code))
            return BadRequest("Bu kod artıq mövcuddur.");

        var discountType = Enum.Parse<DiscountType>(dto.DiscountType);

        var coupon = new Coupon
        {
            Code = dto.Code,
            DiscountType = discountType,
            DiscountValue = dto.DiscountValue,
            MinOrderAmount = dto.MinOrderAmount,
            MaxUsageCount = dto.MaxUsageCount,
            ExpiryDate = dto.ExpiryDate,
            IsActive = true
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), ToDto(coupon));
    }
}
