using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public enum AppointmentStatus
{
    Booked,
    Cancelled,
    Attended,
    NoShow
}
public class Appointment : EntityBase
{
    public Guid AvailabilitySlotId { get; private set; }
    public AvailabilitySlot? AvailabilitySlot { get; private set; }
    public Guid PatientId { get; private set; }
    public Patient? Patient { get; private set; }
    public string Reason { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? AttendedAt { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Appointment() { }
#pragma warning restore CS8618
    #endregion

    public Appointment(Guid availabilitySlotId, Guid patientId, string reason, Guid? id = null) : base(id)
    {
        AvailabilitySlotId = availabilitySlotId;
        PatientId = patientId;
        Reason = reason;
        Status = AppointmentStatus.Booked;
    }

    public void Cancel()
    {
        Status = AppointmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }
}
