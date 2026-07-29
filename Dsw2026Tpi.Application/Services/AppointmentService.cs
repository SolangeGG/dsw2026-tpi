using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<AppointmentModel.Response> Create(AppointmentModel.Request request)
    {
        Validate(request);

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new ValidationException().WithDetail(nameof(request.DoctorId), "not_found");

        var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni);
        if (patient is null)
            throw new ValidationException().WithDetail("patient.dni", "not_found");

        var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId);
        if (slot is null || slot.Deleted || slot.DoctorId != doctor.Id)
            throw new ValidationException().WithDetail(nameof(request.AvailabilityId), "not_found");

        var slotDateTime = slot.SlotDate.ToDateTime(TimeOnly.FromTimeSpan(slot.StartTime));
        if (slotDateTime < DateTime.Now)
            throw new ValidationException().WithDetail(nameof(request.AvailabilityId), "past_slot");

        if (slot.Status != SlotStatus.Available)
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), ErrorCodes.APPOINTMENT_CONFLICT);

        slot.Book();

        try
        {
            await _persistence.Update(slot);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), ErrorCodes.APPOINTMENT_CONFLICT);
        }

        var appointment = new Appointment(slot.Id, patient.Id, request.Reason);
        await _persistence.Add(appointment);

        return new AppointmentModel.Response(appointment.Id, doctor.Id, slot.Id, patient.Dni,
            appointment.Reason, appointment.Status.ToString().ToUpperInvariant());
    }
    private static void Validate(AppointmentModel.Request request)
    {
        var exception = new ValidationException();
        var hasErrors = false;

        if (request.AvailabilityId == Guid.Empty)
        {
            exception.WithDetail(nameof(request.AvailabilityId), "required");
            hasErrors = true;
        }

        var dniLength = request.Patient?.Dni.ToString().Length ?? 0;
        if (request.Patient is null || dniLength is < 7 or > 10)
        {
            exception.WithDetail("patient.dni", "invalid_length");
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
        {
            exception.WithDetail(nameof(request.Reason), "invalid_length");
            hasErrors = true;
        }

        if (hasErrors) throw exception;
    }
    public async Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatientDni(long dni)
    {
        var patient = await _persistence.First<Patient>(p => p.Dni == dni);
        if (patient is null)
            throw new EntityNotFoundException(nameof(Patient));

        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked,
            "AvailabilitySlot.Doctor.Speciality");

        var ordered = (appointments ?? Enumerable.Empty<Appointment>())
            .OrderBy(a => a.AvailabilitySlot!.SlotDate)
            .ThenBy(a => a.AvailabilitySlot!.StartTime);

        return ordered.Select(a => new AppointmentModel.PatientResponse(
            a.Id,
            a.AvailabilitySlot!.Doctor!.Name,
            a.AvailabilitySlot.Doctor.Speciality?.Name,
            a.AvailabilitySlot.SlotDate.ToString("yyyy-MM-dd"),
            a.AvailabilitySlot.StartTime.ToString(@"hh\:mm"),
            a.AvailabilitySlot.EndTime.ToString(@"hh\:mm"),
            a.Reason,
            a.Status.ToString().ToUpperInvariant()
        )).ToList();
    }
    public async Task Cancel(Guid id)
    {
        var appointment = await _persistence.GetById<Appointment>(id);
        if (appointment is null)
            throw new EntityNotFoundException(nameof(Appointment));

        if (appointment.Status != AppointmentStatus.Booked)
            throw new ValidationException().WithDetail(nameof(appointment.Status), "not_cancellable");

        appointment.Cancel();
        await _persistence.Update(appointment);

        var slot = await _persistence.GetById<AvailabilitySlot>(appointment.AvailabilitySlotId);
        if (slot is not null)
        {
            slot.Release();
            await _persistence.Update(slot);
        }
    }
    public async Task<IEnumerable<AppointmentModel.AdminResponse>> GetByDate(string date)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
            throw new ValidationException().WithDetail(nameof(date), "invalid_format");

        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.AvailabilitySlot!.SlotDate == parsedDate,
            "AvailabilitySlot.Doctor.Speciality", "Patient");

        var ordered = (appointments ?? Enumerable.Empty<Appointment>())
            .OrderBy(a => a.AvailabilitySlot!.StartTime);

        return ordered.Select(a => new AppointmentModel.AdminResponse(
            a.Id,
            a.AvailabilitySlot!.Doctor!.Name,
            a.AvailabilitySlot.Doctor.Speciality?.Name,
            a.Patient!.Dni,
            a.Patient.Name,
            a.AvailabilitySlot.SlotDate.ToString("yyyy-MM-dd"),
            a.AvailabilitySlot.StartTime.ToString(@"hh\:mm"),
            a.AvailabilitySlot.EndTime.ToString(@"hh\:mm"),
            a.Reason,
            a.Status.ToString().ToUpperInvariant()
        )).ToList();
    }
    public async Task<Pagination<AppointmentModel.AdminResponse>> Search(
    Guid? specialtyId, Guid? doctorId, long? dni, string? date, int pageSize, int pageIndex)
    {
        DateOnly? parsedDate = null;
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var value))
                throw new ValidationException().WithDetail(nameof(date), "invalid_format");
            parsedDate = value;
        }

        var appointments = await _persistence.Paginate<Appointment, DateOnly>(
            pageSize, pageIndex,
            a => (doctorId == null || a.AvailabilitySlot!.DoctorId == doctorId) &&
                 (specialtyId == null || a.AvailabilitySlot!.Doctor!.SpecialityId == specialtyId) &&
                 (dni == null || a.Patient!.Dni == dni) &&
                 (parsedDate == null || a.AvailabilitySlot!.SlotDate == parsedDate),
            a => a.AvailabilitySlot!.SlotDate,
            "AvailabilitySlot.Doctor.Speciality", "Patient");

        return appointments.Map(a => new AppointmentModel.AdminResponse(
            a.Id,
            a.AvailabilitySlot!.Doctor!.Name,
            a.AvailabilitySlot.Doctor.Speciality?.Name,
            a.Patient!.Dni,
            a.Patient.Name,
            a.AvailabilitySlot.SlotDate.ToString("yyyy-MM-dd"),
            a.AvailabilitySlot.StartTime.ToString(@"hh\:mm"),
            a.AvailabilitySlot.EndTime.ToString(@"hh\:mm"),
            a.Reason,
            a.Status.ToString().ToUpperInvariant()
        ));
    }
}
