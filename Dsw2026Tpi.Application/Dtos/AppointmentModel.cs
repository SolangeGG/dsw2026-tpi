using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record PatientRequest(long Dni);
    public record Request(Guid DoctorId, Guid AvailabilityId, PatientRequest Patient, string Reason);
    public record Response(Guid Id, Guid DoctorId, Guid AvailabilityId, long PatientDni, string Reason, string Status);
}

