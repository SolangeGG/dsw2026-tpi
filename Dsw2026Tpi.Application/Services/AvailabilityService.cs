using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;


namespace Dsw2026Tpi.Application.Services;

    public class AvailabilityService : IAvailabilityService
    {
    private readonly IPersistence _persistence;

    public AvailabilityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task Create(AvailabilityModel.Request request)
    {
        var doctor = await ValidateAndGetDoctor(request);
        var parsedDays = ParseDays(request.Days);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var (dayOfWeek, startTime, endTime) in parsedDays)
        {
            var existingRule = await _persistence.First<AvailabilityRule>(
                r => r.DoctorId == doctor.Id && r.Month == today.Month && r.Year == today.Year &&
                     r.DayOfWeek == dayOfWeek && !r.Deleted);

            if (existingRule is not null)
                throw new ConflictException(nameof(ErrorCodes.AVAILABILITY_CONFLICT), ErrorCodes.AVAILABILITY_CONFLICT);

            var rule = new AvailabilityRule(doctor.Id, today.Month, today.Year, dayOfWeek, startTime, endTime);
            await _persistence.Add(rule);

            await GenerateSlots(doctor.Id, rule.Id, dayOfWeek, startTime, endTime, today);
        }
    }
    private async Task<Doctor> ValidateAndGetDoctor(AvailabilityModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new ValidationException().WithDetail(nameof(request.DoctorId), "not_found");

        if (request.Days is null || request.Days.Count == 0)
            throw new ValidationException().WithDetail(nameof(request.Days), "required");

        var duplicated = request.Days
            .GroupBy(d => d.Day.Trim().ToUpperInvariant())
            .Any(g => g.Count() > 1);

        if (duplicated)
            throw new ValidationException().WithDetail(nameof(request.Days), "duplicated_day");

        return doctor;
    }

    private static List<(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime)> ParseDays(
       IEnumerable<AvailabilityModel.DayRequest> days)
    {
        var exception = new ValidationException();
        var hasErrors = false;
        var result = new List<(DayOfWeek, TimeSpan, TimeSpan)>();

        foreach (var day in days)
        {
            if (!TryParseDay(day.Day, out var dayOfWeek))
            {
                exception.WithDetail(nameof(day.Day), "invalid_day");
                hasErrors = true;
                continue;
            }

            if (!TimeSpan.TryParseExact(day.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var startTime) ||
                !TimeSpan.TryParseExact(day.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var endTime))
            {
                exception.WithDetail(nameof(day.StartTime), "invalid_format");
                hasErrors = true;
                continue;
            }

            if (startTime >= endTime)
            {
                exception.WithDetail(nameof(day.StartTime), "start_after_end");
                hasErrors = true;
                continue;
            }

            result.Add((dayOfWeek, startTime, endTime));
        }

        if (hasErrors) throw exception;

        return result;
    }

    private static bool TryParseDay(string day, out DayOfWeek dayOfWeek)
    {
        dayOfWeek = day?.Trim().ToUpperInvariant() switch
        {
            "LUNES" => DayOfWeek.Monday,
            "MARTES" => DayOfWeek.Tuesday,
            "MIÉRCOLES" or "MIERCOLES" => DayOfWeek.Wednesday,
            "JUEVES" => DayOfWeek.Thursday,
            "VIERNES" => DayOfWeek.Friday,
            "SÁBADO" or "SABADO" => DayOfWeek.Saturday,
            "DOMINGO" => DayOfWeek.Sunday,
            _ => (DayOfWeek)(-1)
        };

        return (int)dayOfWeek != -1;
    }

    private async Task GenerateSlots(Guid doctorId, Guid ruleId, DayOfWeek dayOfWeek,
        TimeSpan startTime, TimeSpan endTime, DateOnly today)
    {
        var lastDayOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        for (var date = today; date <= lastDayOfMonth; date = date.AddDays(1))
        {
            if (date.DayOfWeek != dayOfWeek) continue;

            for (var slotStart = startTime; slotStart < endTime; slotStart = slotStart.Add(TimeSpan.FromMinutes(30)))
            {
                var existingSlot = await _persistence.First<AvailabilitySlot>(
                    s => s.DoctorId == doctorId && s.SlotDate == date && s.StartTime == slotStart);

                if (existingSlot is not null) continue;

                var slotEnd = slotStart.Add(TimeSpan.FromMinutes(30));
                var slot = new AvailabilitySlot(doctorId, ruleId, date, slotStart, slotEnd);
                await _persistence.Add(slot);
            }
        }
    }
    public async Task Update(AvailabilityModel.Request request)
    {
        var doctor = await ValidateAndGetDoctor(request);
        var parsedDays = ParseDays(request.Days);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await ClearCurrentMonth(doctor.Id, today);

        foreach (var (dayOfWeek, startTime, endTime) in parsedDays)
        {
            var rule = new AvailabilityRule(doctor.Id, today.Month, today.Year, dayOfWeek, startTime, endTime);
            await _persistence.Add(rule);

            await GenerateSlots(doctor.Id, rule.Id, dayOfWeek, startTime, endTime, today);
        }
    }

    private async Task ClearCurrentMonth(Guid doctorId, DateOnly today)
    {
        var rules = await _persistence.GetFiltered<AvailabilityRule>(
            r => r.DoctorId == doctorId && r.Month == today.Month && r.Year == today.Year && !r.Deleted);

        foreach (var rule in rules ?? Enumerable.Empty<AvailabilityRule>())
        {
            rule.Delete();
            await _persistence.Update(rule);
        }

        var lastDayOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var slots = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.DoctorId == doctorId && s.SlotDate >= today && s.SlotDate <= lastDayOfMonth &&
                 s.Status == SlotStatus.Available && !s.Deleted);

        foreach (var slot in slots ?? Enumerable.Empty<AvailabilitySlot>())
        {
            slot.Delete();
            await _persistence.Update(slot);
        }
    }

}

