using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record PatientRequest(long Dni);
    public record Request(Guid DoctorId, Guid AvailabilityId, PatientRequest Patient, string Reason);
    public record Response(Guid Id, Guid DoctorId, Guid AvailabilityId, long PatientDni, string Reason, string Status);
    public record PatientResponse(Guid Id, string DoctorName, string? SpecialityName,
        string Date, string StartTime, string EndTime, string Reason, string Status);
}

