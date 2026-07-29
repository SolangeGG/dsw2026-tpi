using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.Response> Create(AppointmentModel.Request request);
    Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatientDni(long dni);
    Task Cancel(Guid id);
    Task<IEnumerable<AppointmentModel.AdminResponse>> GetByDate(string date);
}

