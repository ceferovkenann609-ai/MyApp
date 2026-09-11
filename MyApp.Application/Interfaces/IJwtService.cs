using MyApp.Domain.Entities;

namespace MyApp.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
