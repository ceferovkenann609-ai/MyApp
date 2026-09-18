using Microsoft.EntityFrameworkCore;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;

namespace MyApp.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly AppDbContext _context;

    public IdempotencyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyKey?> GetAsync(string key)
    {
        return await _context.IdempotencyKeys.FirstOrDefaultAsync(k => k.Key == key);
    }

    public async Task<bool> TrySaveAsync(string key, string requestPath, int statusCode, string responseBody)
    {
        try
        {
            _context.IdempotencyKeys.Add(new IdempotencyKey
            {
                Key = key,
                RequestPath = requestPath,
                ResponseStatusCode = statusCode,
                ResponseBody = responseBody,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
