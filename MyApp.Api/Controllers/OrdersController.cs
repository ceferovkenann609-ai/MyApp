using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.DTOs;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IValidator<UpdateOrderStatusDto> _statusValidator;

    public OrdersController(
        AppDbContext context,
        IValidator<CreateOrderDto> createValidator,
        IValidator<UpdateOrderStatusDto> statusValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (!IsAdmin)
            query = query.Where(o => o.UserId == CurrentUserId);

        var orders = await query.ToListAsync();
        return Ok(orders.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        if (!IsAdmin && order.UserId != CurrentUserId)
            return Forbid();

        return Ok(MapToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var userId = CurrentUserId;
        var strategy = _context.Database.CreateExecutionStrategy();

        Order? createdOrder = null;
        string? errorMessage = null;

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending
                };

                decimal total = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive);
                    if (product == null)
                    {
                        errorMessage = $"ProductId {item.ProductId} mövcud deyil.";
                        await transaction.RollbackAsync();
                        return;
                    }

                    if (product.Stock < item.Quantity)
                    {
                        errorMessage = $"'{product.Name}' üçün kifayət qədər stok yoxdur. Mövcud: {product.Stock}, İstənilən: {item.Quantity}";
                        await transaction.RollbackAsync();
                        return;
                    }

                    var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE \"Products\" SET \"Stock\" = \"Stock\" - {item.Quantity} WHERE \"Id\" = {product.Id} AND \"Stock\" >= {item.Quantity}");

                    if (rowsAffected == 0)
                    {
                        errorMessage = $"'{product.Name}' üçün stok başqa bir sifariş tərəfindən dəyişdirildi. Yenidən cəhd edin.";
                        await transaction.RollbackAsync();
                        return;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    order.OrderItems.Add(orderItem);
                    total += product.Price * item.Quantity;
                }

                if (errorMessage != null) return;

                order.TotalAmount = total;
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    Status = OrderStatus.Pending,
                    ChangedAt = DateTime.UtcNow,
                    Note = "Sifariş yaradıldı."
                });

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                createdOrder = order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        if (errorMessage != null)
            return BadRequest(errorMessage);

        if (createdOrder == null)
            return StatusCode(500, "Sifariş yaradıla bilmədi.");

        var full = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == createdOrder.Id);

        return CreatedAtAction(nameof(GetById), new { id = full.Id }, MapToDto(full));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        await _statusValidator.ValidateAndThrowAsync(dto);

        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Yanlış status dəyəri. (Pending, Confirmed, Shipped, Delivered, Cancelled)");

        if (!OrderStatusTransitionRules.IsValid(order.Status, newStatus))
            return BadRequest($"'{order.Status}' statusundan '{newStatus}' statusuna keçidə icazə verilmir.");

        order.Status = newStatus;

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = newStatus,
            ChangedAt = DateTime.UtcNow,
            Note = dto.Note
        });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            UserId = o.UserId,
            UserName = o.User.Name,
            Items = o.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };
    }
}
