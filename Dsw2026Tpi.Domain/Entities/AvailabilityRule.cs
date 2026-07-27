using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilityRule : EntityBase
{
    public Guid DoctorId { get; private set; }
    public Doctor? Doctor { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public bool Deleted { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private AvailabilityRule() { }
#pragma warning restore CS8618
    #endregion

    public AvailabilityRule(Guid doctorId, int month, int year, DayOfWeek dayOfWeek,
        TimeSpan startTime, TimeSpan endTime, Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        Month = month;
        Year = year;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        Deleted = false;
    }

    public void Delete()
    {
        Deleted = true;
    }

}
