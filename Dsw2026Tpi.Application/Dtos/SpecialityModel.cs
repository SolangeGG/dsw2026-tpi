using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record SpecialityModel
    {
        public record Response(Guid Id, string Name, string Description);
    }
}
