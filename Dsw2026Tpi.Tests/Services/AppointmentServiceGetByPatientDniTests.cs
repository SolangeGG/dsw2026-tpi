using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Tests.Services
{
    public class AppointmentServiceGetByPatientDniTests
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<AppointmentService> _logger;
        private readonly AppointmentService _service;

        public AppointmentServiceGetByPatientDniTests()
        {
            _persistence = Substitute.For<IPersistence>();
            _logger = Substitute.For<ILogger<AppointmentService>>();
            _service = new AppointmentService(_persistence, _logger);
        }

        [Fact]
        public async Task ObtenerPorDni_CuandoElPacienteNoExiste_EntoncesLanzaExcepcionDeNoEncontrado()
        {
            long dniInexistente = 30123456;

            _persistence.First<Patient>(Arg.Any<System.Linq.Expressions.Expression<Func<Patient, bool>>>())
                .Returns((Patient?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetByPatientDni(dniInexistente));
        }
    }
}
