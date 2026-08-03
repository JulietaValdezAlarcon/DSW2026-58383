using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Data.Configurations // O el namespace que uses para configuraciones
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.DateOfService)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            // Relación con Patient
            builder.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments) // Asegúrate de que Patient tenga esta colección
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Turn
            builder.HasOne(a => a.Turn)
                .WithMany() // O WithOne dependiendo de cómo lo maneje Turn
                .HasForeignKey(a => a.TurnId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}