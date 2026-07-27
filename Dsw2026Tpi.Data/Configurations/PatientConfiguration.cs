using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DNI)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(200);

        builder.Property(p => p.TelephoneNumber)
            .IsRequired();
        builder.HasIndex(p => p.DNI).IsUnique();

        builder.HasMany(p => p.appointments)
            .WithOne()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
