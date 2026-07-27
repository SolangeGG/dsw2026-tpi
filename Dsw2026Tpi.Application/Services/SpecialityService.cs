using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

    public class SpecialityService : ISpecialityService
    {
        private readonly IPersistence _persistence;

        public SpecialityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
        {
            if (!string.IsNullOrWhiteSpace(name) && name.Length is < 3 or > 100)
            {
                throw new ValidationException().WithDetail(nameof(name), "invalid_length");
            }

        var specialities = await _persistence.Paginate<Speciality, string>( pageSize, pageIndex, s => !s.Deleted &&
     (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)),s => s.Name);

        return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
        }

    public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
    {
        Validate(request);
        await EnsureNameIsUnique(request.Name);

        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);

        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }
    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id);
        if (speciality is null || speciality.Deleted)
            throw new EntityNotFoundException(nameof(Speciality));

        speciality.Delete();
        await _persistence.Update(speciality);
    }
    private async Task EnsureNameIsUnique(string name)
    {
        var existing = await _persistence.First<Speciality>(
            s => s.Deleted && s.Name.ToLower() == name.ToLower());

        if (existing is not null)
            throw new ConflictException(nameof(ErrorCodes.SPECIALITY_CONFLICT), ErrorCodes.SPECIALITY_CONFLICT);
    }

    private static void Validate(SpecialityModel.Request request)
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

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            exception.WithDetail(nameof(request.Description), "required");
            hasErrors = true;
        }
        else if (request.Description.Length is < 10 or > 100)
        {
            exception.WithDetail(nameof(request.Description), "invalid_length");
            hasErrors = true;
        }

        if (hasErrors) throw exception;
    }
    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        Validate(request);

        var speciality = await _persistence.GetById<Speciality>(id);
        if (speciality is null || speciality.Deleted)
            throw new EntityNotFoundException(nameof(Speciality));

        await EnsureNameIsUnique(request.Name, id);

        speciality.Update(request.Name, request.Description);
        await _persistence.Update(speciality);

        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }
    private async Task EnsureNameIsUnique(string name, Guid? excludeId = null)
    {
        var existing = await _persistence.First<Speciality>(
            s => !s.Deleted && s.Name.ToLower() == name.ToLower());

        if (existing is not null && existing.Id != excludeId)
            throw new ConflictException(nameof(ErrorCodes.SPECIALITY_CONFLICT), ErrorCodes.SPECIALITY_CONFLICT);
    }

}

