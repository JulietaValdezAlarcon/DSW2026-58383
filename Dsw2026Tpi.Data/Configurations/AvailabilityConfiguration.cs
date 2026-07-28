using System;
using System.Collections.Generic;
using System.Text;

using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilityConfiguration
    : IEntityTypeConfiguration<Availability>
{
    public void Configure(EntityTypeBuilder<Availability> builder)
    {
        builder.ToTable("Availabilities");

        builder.HasKey(availability => availability.Id);

        builder.Property(availability => availability.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(availability => availability.StartTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(availability => availability.EndTime)
            .HasColumnType("time(0)")
            .IsRequired();

        builder.Property(availability => availability.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(availability => availability.CreatedAt)
            .IsRequired();

        builder.Property(availability => availability.UpdatedAt)
            .IsRequired();

        builder.HasOne(availability => availability.Doctor)
            .WithMany(doctor => doctor.Availabilities)
            .HasForeignKey(availability => availability.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(availability => new
        {
            availability.DoctorId,
            availability.Date,
            availability.StartTime
        })
            .IsUnique();
    }
}
