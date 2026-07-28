using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;


public enum SlotStatus
{
    Available,
    Booked,
    Blocked
}

public class AvailabilitySlot : EntityBase
    {
    public Guid DoctorId { get; private set; }
    public Doctor? Doctor { get; private set; }
    public Guid? AvailabilityRuleId { get; private set; }
    public DateOnly SlotDate { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public SlotStatus Status { get; private set; }
    public bool Deleted { get; private set; }
    public byte[]? RowVersion { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private AvailabilitySlot() { }
#pragma warning restore CS8618
    #endregion

    public AvailabilitySlot(Guid doctorId, Guid? availabilityRuleId, DateOnly slotDate,
        TimeSpan startTime, TimeSpan endTime, Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        AvailabilityRuleId = availabilityRuleId;
        SlotDate = slotDate;
        StartTime = startTime;
        EndTime = endTime;
        Status = SlotStatus.Available;
        Deleted = false;
    }

    public void Delete()
    {
        Deleted = true;
    }
    public void Book()
    {
        Status = SlotStatus.Booked;
    }
}

