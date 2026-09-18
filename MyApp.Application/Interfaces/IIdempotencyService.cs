using MyApp.Domain.Entities;

namespace MyApp.Application.Interfaces;

public interface IIdempotencyService
{
    Task<IdempotencyKey?> GetAsync(string key);
    Task<bool> TrySaveAsync(string key, string requestPath, int statusCode, string responseBody);
}
