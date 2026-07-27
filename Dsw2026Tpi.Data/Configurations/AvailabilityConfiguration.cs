using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
{
    public void Configure(EntityTypeBuilder<Availability> builder)
    {
        builder.ToTable("Availabilities");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(a => a.StartTime)
            .HasColumnType("time");

        builder.Property(a => a.EndTime)
            .HasColumnType("time");

        builder.HasMany(a => a.Turns)
            .WithOne()
            .HasForeignKey(t => t.AvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.Date, a.DoctorId });
    }
}
