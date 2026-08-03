using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
    {
        public void Configure(EntityTypeBuilder<Availability> builder)
        {
            builder.ToTable("Availabilities");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Date)
                .IsRequired();

            builder.Property(a => a.StartTime)
                .HasColumnType("time(0)")
                .IsRequired();

            builder.Property(a => a.EndTime)
                .HasColumnType("time(0)")
                .IsRequired();

            builder.Property(a => a.Status)
                .HasConversion<string>() // O almacénalo como int si tu enum es numérico, por defecto string con max length es seguro
                .HasMaxLength(20)
                .IsRequired();

            // Relación limpia con Doctor (Evita columnas duplicadas)
            builder.HasOne(a => a.Doctor)
                .WithMany(d => d.Availabilities) // Asegúrate de que Doctor tenga esta colección (ICollection<Availability>)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Turns (si Availability tiene una colección de Turns)
            builder.HasMany(a => a.Turns)
                .WithOne()
                .HasForeignKey(t => t.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único compuesto para evitar solapamientos exactos de horario por médico
            builder.HasIndex(a => new { a.DoctorId, a.Date, a.StartTime })
                .IsUnique();
        }
    }
}
