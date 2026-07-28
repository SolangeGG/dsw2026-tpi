using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

    public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
    {
      public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
      {
        builder.ToTable("AvailabilitySlots");

        builder.Property(s => s.Status)
            .HasConversion<string>();

        builder.HasOne(s => s.Doctor)
            .WithMany()
            .HasForeignKey(s => s.DoctorId);

        builder.HasIndex(s => new { s.DoctorId, s.SlotDate, s.StartTime })
            .IsUnique();
      }
    }

