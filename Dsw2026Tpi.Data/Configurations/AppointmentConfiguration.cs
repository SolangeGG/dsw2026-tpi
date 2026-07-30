using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.Property(a => a.Status).HasConversion<string>();

        builder.HasOne(a => a.AvailabilitySlot)
            .WithMany()
            .HasForeignKey(a => a.AvailabilitySlotId);

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId);

        builder.HasIndex(a => a.AvailabilitySlotId)
          .IsUnique()
          .HasFilter("[Status] = 'Booked'");
    }
}
