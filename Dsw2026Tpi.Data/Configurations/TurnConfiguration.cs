using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.ToTable("Turns");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Date)
            .HasColumnType("date");

        builder.Property(t => t.StartTime)
            .HasColumnType("datetime2");

        builder.Property(t => t.EndTime)
            .HasColumnType("datetime2");

        builder.Property(t => t.Status)
            .HasConversion<int>();

        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(t => t.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.AvailabilityId, t.Status });
    }
}
