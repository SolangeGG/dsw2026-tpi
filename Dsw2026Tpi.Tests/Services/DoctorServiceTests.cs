using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly IPersistence _persistence;
        private readonly DoctorService _service;

        public DoctorServiceTests()
        {
            _persistence = Substitute.For<IPersistence>();
            _service = new DoctorService(_persistence);
        }
        [Fact]
        public async Task Eliminar_CuandoElMedicoExiste_EntoncesLoMarcaComoEliminado()
        {
            var speciality = new Speciality("Cardiología", "Especialidad del corazón");
            var doctor = new Doctor("Dr. Juan Pérez", "MP12345", speciality);

            _persistence.GetById<Doctor>(doctor.Id).Returns(doctor);

            await _service.Delete(doctor.Id);

            Assert.True(doctor.Deleted);
            await _persistence.Received(1).Update(doctor);
        }
    }
}
