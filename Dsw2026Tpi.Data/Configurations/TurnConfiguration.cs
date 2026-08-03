using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Data.Configurations
{
    public class TurnConfiguration : IEntityTypeConfiguration<Turn>
    {
        public void Configure(EntityTypeBuilder<Turn> builder)
        {
            builder.ToTable("Turns");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Date)
                .IsRequired();

            builder.Property(t => t.StartTime)
                .IsRequired();

            builder.Property(t => t.EndTime)
                .IsRequired();

            builder.Property(t => t.Status)
                .IsRequired();

            // Configuración vital para la concurrencia optimista en SQL Server
            builder.Property(t => t.RowVersion)
                .IsRowVersion();

            // Relación con Availability
            builder.HasOne<Availability>()
                .WithMany(a => a.Turns)
                .HasForeignKey(t => t.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación opcional con Appointment
            builder.HasOne(t => t.Appointment)
                .WithOne(a => a.Turn)
                .HasForeignKey<Appointment>(a => a.TurnId)
                .OnDelete(DeleteBehavior.SetNull);

            // Índice para optimizar búsquedas por disponibilidad y estado
            builder.HasIndex(t => new { t.AvailabilityId, t.Status });
        }
    }
}