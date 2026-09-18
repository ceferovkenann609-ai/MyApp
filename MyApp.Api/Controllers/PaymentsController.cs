using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly IIdempotencyService _idempotencyService;

    public PaymentsController(AppDbContext context, IPaymentService paymentService, IIdempotencyService idempotencyService)
    {
        _context = context;
        _paymentService = paymentService;
        _idempotencyService = idempotencyService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpPost("orders/{orderId}/pay")]
    public async Task<IActionResult> Pay(int orderId, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest("Idempotency-Key header tələb olunur.");

        var path = $"POST /api/payments/orders/{orderId}/pay";

        var existing = await _idempotencyService.GetAsync(idempotencyKey);
        if (existing != null)
        {
            return new ContentResult
            {
                StatusCode = existing.ResponseStatusCode,
                Content = existing.ResponseBody,
                ContentType = "application/json"
            };
        }

        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return await RespondAndStore(idempotencyKey, path, 404, new { message = "Sifariş tapılmadı." });

        if (!IsAdmin && order.UserId != CurrentUserId)
            return await RespondAndStore(idempotencyKey, path, 403, new { message = "Bu sifarişə giriş icazəniz yoxdur." });

        if (order.Payments.Any(p => p.Status == PaymentStatus.Completed))
            return await RespondAndStore(idempotencyKey, path, 400, new { message = "Bu sifariş artıq ödənilib." });

        if (order.Status != OrderStatus.Pending)
            return await RespondAndStore(idempotencyKey, path, 400, new { message = $"'{order.Status}' statuslu sifariş üçün ödəniş qəbul edilmir." });

        var chargeResult = await _paymentService.ChargeAsync(order.FinalAmount);

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.FinalAmount,
            Status = chargeResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed,
            TransactionReference = chargeResult.TransactionReference,
            FailureReason = chargeResult.FailureReason,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        if (chargeResult.Success)
        {
            order.Status = OrderStatus.Confirmed;
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Confirmed,
                ChangedAt = DateTime.UtcNow,
                Note = "Ödəniş uğurla tamamlandı."
            });
        }

        await _context.SaveChangesAsync();

        var dto = new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            TransactionReference = payment.TransactionReference,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt
        };

        var statusCode = chargeResult.Success ? 200 : 402;
        return await RespondAndStore(idempotencyKey, path, statusCode, dto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return NotFound();

        if (!IsAdmin && payment.Order.UserId != CurrentUserId)
            return Forbid();

        return Ok(new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            TransactionReference = payment.TransactionReference,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt
        });
    }

    private async Task<IActionResult> RespondAndStore(string key, string path, int statusCode, object body)
    {
        var json = JsonSerializer.Serialize(body);
        await _idempotencyService.TrySaveAsync(key, path, statusCode, json);

        return new ContentResult
        {
            StatusCode = statusCode,
            Content = json,
            ContentType = "application/json"
        };
    }
}
