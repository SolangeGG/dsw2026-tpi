using Dsw2026Tpi.Api.Configurations;
using Dsw2026Tpi.Api.Middlewares;
using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Serilog;
using System.Text.Json;
using System.Threading.RateLimiting;



namespace Dsw2026Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Inicializar con un logger simple antes de construir el host
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Iniciando aplicación Dsw2026Tpi.Api");

            var builder = WebApplication.CreateBuilder(args);

            //Configuraciones personalizadas
            builder.AddSerilogConfiguration();
            builder.Services.AddAppIdentity();
            builder.Services.AddAppAuthentication(builder.Configuration);
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddApplicationPersistence(builder.Configuration);
            builder.Services.AddAppCors(builder.Configuration);
            builder.Services.AddAppDependencies();
            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();

            var rateLimitingOptions = builder.Configuration
    .GetSection(RateLimitingOptions.SectionName)
    .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingOptions.Global.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingOptions.Global.WindowSeconds),
                            QueueLimit = 0
                        }));

                options.AddPolicy("AdminLogin", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingOptions.AdminLogin.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingOptions.AdminLogin.WindowSeconds),
                            QueueLimit = 0
                        }));

                options.AddPolicy("PatientLogin", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingOptions.PatientLogin.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingOptions.PatientLogin.WindowSeconds),
                            QueueLimit = 0
                        }));

                options.AddPolicy("AppointmentBooking", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitingOptions.AppointmentBooking.PermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitingOptions.AppointmentBooking.WindowSeconds),
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var endpoint = context.HttpContext.GetEndpoint()?.DisplayName ?? context.HttpContext.Request.Path.ToString();
                    var origin = context.HttpContext.User.Identity?.Name
                        ?? context.HttpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "desconocido";

                    logger.LogWarning("Rate limit excedido: endpoint {Endpoint}, origen {Origin}", endpoint, origin);

                    var error = new ErrorResponse(nameof(ErrorCodes.RATE_LIMIT_EXCEEDED), ErrorCodes.RATE_LIMIT_EXCEEDED);
                    var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(json, cancellationToken);
                };
            });

            var app = builder.Build();

            await AdminInitializer.InitializeAdminAsync(app.Services, app.Configuration);

            app.UseSerilogRequestLogging();

            if (app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.MapControllers();
            app.MapHealthChecks("/health-check");

            Log.Information("Aplicación iniciada correctamente");

            await app.RunAsync();
        }
        catch (HostAbortedException)
        {
            Log.Information("El host fue abortado (normal durante migraciones de EF Core)");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "La aplicación falló al iniciar");
            throw;
        }
        finally
        {
            Log.Information("Cerrando aplicación");
            await Log.CloseAndFlushAsync();
        }
    }
}

