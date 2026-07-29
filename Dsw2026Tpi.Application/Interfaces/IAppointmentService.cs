using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.Response> Create(AppointmentModel.Request request);
    Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatientDni(long dni);
    Task Cancel(Guid id);
    Task<IEnumerable<AppointmentModel.AdminResponse>> GetByDate(string date);
    Task<Pagination<AppointmentModel.AdminResponse>> Search(
    Guid? specialtyId, Guid? doctorId, long? dni, string? date, int pageSize, int pageIndex);
}

