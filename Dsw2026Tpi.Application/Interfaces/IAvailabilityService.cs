using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;


namespace Dsw2026Tpi.Application.Interfaces;

    public interface IAvailabilityService
    {
    Task Create(AvailabilityModel.Request request);

    }

