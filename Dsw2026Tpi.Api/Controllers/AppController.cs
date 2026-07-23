using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

/// <summary>
/// Clase base para configuraciones generales de controladores
/// </summary>
[ApiController]
[Route("api")]
[EnableRateLimiting("fixed")]
public abstract class AppController : ControllerBase
{
}

