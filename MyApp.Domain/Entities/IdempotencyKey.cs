namespace MyApp.Domain.Entities;

public class IdempotencyKey
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public int ResponseStatusCode { get; set; }
    public string ResponseBody { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
