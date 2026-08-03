using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Data.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relación opcional con Speciality
            builder.HasOne(d => d.Speciality)
                .WithMany()
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación con Availabilities
            builder.HasMany(d => d.Availabilities)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}