using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.DNI)
                .IsRequired();

            // Índice único para evitar DNI duplicados
            builder.HasIndex(p => p.DNI)
                .IsUnique();

            builder.Property(p => p.Name)
                .HasMaxLength(200);

            builder.Property(p => p.TelephoneNumber)
                .IsRequired();

            // Relación con Appointments
            builder.HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}