using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record PatientRequest(long Dni);
    public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientRequest Patient, string Reason);
    public record Response(Guid Id, Guid DoctorId, Guid AvailabilitySlotId, long PatientDni, string Reason, string Status);
    public record PatientResponse(Guid Id, string DoctorName, string? SpecialityName,
        string Date, string StartTime, string EndTime, string Reason, string Status);
    public record SpecialtyDto(Guid? SpecialtyId, string? Name);
    public record DoctorDto(Guid DoctorId, string Name, SpecialtyDto? Specialty);
    public record PatientDto(long Dni, string FullName);
    public record AdminResponse(Guid AppointmentsId, string AppointmentsStatus,
        PatientDto Patient, DoctorDto Doctor);
}

