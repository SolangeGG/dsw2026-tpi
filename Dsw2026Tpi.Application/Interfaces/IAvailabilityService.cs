using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;


namespace Dsw2026Tpi.Application.Interfaces;

    public interface IAvailabilityService
    {
    Task<List<AvailabilityModel.RuleResponse>> Create(AvailabilityModel.Request request);
    Task<List<AvailabilityModel.RuleResponse>> Update(AvailabilityModel.Request request);

}

