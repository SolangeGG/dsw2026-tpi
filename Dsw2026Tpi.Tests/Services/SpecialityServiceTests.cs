using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.Services
{
    public class SpecialityServiceTests
    {
        private readonly IPersistence _persistence;
        private readonly SpecialityService _service;

        public SpecialityServiceTests()
        {
            _persistence = Substitute.For<IPersistence>();
            _service = new SpecialityService(_persistence);
        }

        [Fact]
        public async Task Crear_CuandoLosDatosSonValidos_EntoncesDevuelveLaEspecialidadCreada()
        {
            //Arrange
            _persistence.First<Speciality>(Arg.Any<Expression<Func<Speciality, bool>>>())
                .Returns((Speciality?)null);

            var request = new SpecialityModel.Request("Cardiología", "Especialidad del corazón");

            //Act
            var result = await _service.Create(request);

            //Assert
            Assert.Equal("Cardiología", result.Name);
            Assert.Equal("Especialidad del corazón", result.Description);
            await _persistence.Received(1).Add(Arg.Is<Speciality>(s => s!.Name == "Cardiología"));
        }
    }
}
