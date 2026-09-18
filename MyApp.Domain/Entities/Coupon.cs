namespace MyApp.Domain.Entities;

public enum DiscountType
{
    Percentage,
    FixedAmount
}

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public int? MaxUsageCount { get; set; }
    public int UsedCount { get; set; } = 0;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
