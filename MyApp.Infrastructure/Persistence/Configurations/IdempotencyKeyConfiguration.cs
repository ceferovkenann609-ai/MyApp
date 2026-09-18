using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain.Entities;

namespace MyApp.Infrastructure.Persistence.Configurations;

public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.Property(i => i.Key).IsRequired().HasMaxLength(200);
        builder.HasIndex(i => i.Key).IsUnique();
        builder.Property(i => i.RequestPath).HasMaxLength(300);
        builder.Property(i => i.ResponseBody).HasColumnType("text");
    }
}
