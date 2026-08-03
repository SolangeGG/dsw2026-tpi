using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<AppointmentService> _logger;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _persistence = Substitute.For<IPersistence>();
            _logger = Substitute.For<ILogger<AppointmentService>>();
            _service = new AppointmentService(_persistence, _logger);
        }
        [Fact]
        public async Task Cancelar_CuandoElTurnoEstaReservado_EntoncesLoCancelaYLiberaElSlot()
        {
            var slot = new AvailabilitySlot(Guid.NewGuid(), null,
                DateOnly.FromDateTime(DateTime.Today.AddDays(1)), TimeSpan.FromHours(9), TimeSpan.FromHours(9.5));
            slot.Book();

            var appointment = new Appointment(slot.Id, Guid.NewGuid(), "Control de rutina");

            _persistence.GetById<Appointment>(appointment.Id).Returns(appointment);
            _persistence.GetById<AvailabilitySlot>(slot.Id).Returns(slot);

            await _service.Cancel(appointment.Id);

            Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
            Assert.Equal(SlotStatus.Available, slot.Status);
            await _persistence.Received(1).Update(appointment);
            await _persistence.Received(1).Update(slot);
        }
    }
}
