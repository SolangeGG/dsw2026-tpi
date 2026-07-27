using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex,
            d => d.IsActive && (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)),
            x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }
    public async Task<IEnumerable<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid doctorId)
    {
        var doctor = await _persistence.GetById<Doctor>(doctorId);
        if (doctor is null || !doctor.IsActive)
            throw new EntityNotFoundException(nameof(Doctor));

        var now = DateTime.UtcNow;

        var rules = await _persistence.GetFiltered<AvailabilityRule>(
            r => r.DoctorId == doctorId && r.Month == now.Month && r.Year == now.Year && !r.Deleted);

        return (rules ?? Enumerable.Empty<AvailabilityRule>())
            .Select(r => new DoctorModel.AvailabilityResponse(
                ToSpanishDay(r.DayOfWeek),
                r.StartTime.ToString(@"hh\:mm"),
                r.EndTime.ToString(@"hh\:mm")))
            .ToList();
    }

    private static string ToSpanishDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "LUNES",
        DayOfWeek.Tuesday => "MARTES",
        DayOfWeek.Wednesday => "MIÉRCOLES",
        DayOfWeek.Thursday => "JUEVES",
        DayOfWeek.Friday => "VIERNES",
        DayOfWeek.Saturday => "SÁBADO",
        DayOfWeek.Sunday => "DOMINGO",
        _ => throw new ArgumentOutOfRangeException(nameof(day))
    };

    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        Validate(request);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
        if (speciality is null || speciality.Deleted)
            throw new ValidationException().WithDetail(nameof(request.SpecialityId), "not_found");

        var doctor = new Doctor(request.Name, request.LicenseNumber, speciality);
        await _persistence.Add(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }

    private static void Validate(DoctorModel.Request request)
    {
        var exception = new ValidationException();
        var hasErrors = false;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            exception.WithDetail(nameof(request.Name), "required");
            hasErrors = true;
        }
        else if (request.Name.Length is < 3 or > 100)
        {
            exception.WithDetail(nameof(request.Name), "invalid_length");
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            exception.WithDetail(nameof(request.LicenseNumber), "required");
            hasErrors = true;
        }

        if (hasErrors) throw exception;
    }
    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);
        if (doctor is null || !doctor.IsActive)
            throw new EntityNotFoundException(nameof(Doctor));

        doctor.Deactivate();
        await _persistence.Update(doctor);
    }
    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        Validate(request);

        var doctor = await _persistence.GetById<Doctor>(id);
        if (doctor is null || !doctor.IsActive)
            throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
        if (speciality is null || speciality.Deleted)
            throw new ValidationException().WithDetail(nameof(request.SpecialityId), "not_found");

        doctor.Update(request.Name, request.LicenseNumber, speciality);
        await _persistence.Update(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }
}
