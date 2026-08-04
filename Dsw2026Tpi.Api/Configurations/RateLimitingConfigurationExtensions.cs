namespace Dsw2026Tpi.Api.Configurations;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RateLimitingSettings");

        // Leer valores de configuración (con valores por defecto si no existen)
        int adminLimit = section.GetValue<int>("AdminLoginLimit", 5);
        int patientLimit = section.GetValue<int>("PatientLoginLimit", 10);
        int appointmentLimit = section.GetValue<int>("AppointmentLimit", 5);
        int generalLimit = section.GetValue<int>("GeneralLimit", 100);

        services.AddRateLimiter(options =>
        {
            // 1. Admin Login Policy (5 req / min)[cite: 1]
            options.AddFixedWindowLimiter("AdminLoginPolicy", opt =>
            {
                opt.PermitLimit = adminLimit;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0; // No encolar[cite: 1]
            });

            // 2. Patient Login Policy (10 req / min)[cite: 1]
            options.AddFixedWindowLimiter("PatientLoginPolicy", opt =>
            {
                opt.PermitLimit = patientLimit;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            // 3. Appointment Policy (5 req / min por paciente)[cite: 1]
            options.AddPolicy("AppointmentPolicy", httpContext =>
            {
                string clientId = httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientId, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = appointmentLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            // 4. General Policy (100 req / min)[cite: 1]
            options.AddPolicy("GeneralPolicy", httpContext =>
            {
                string clientId = httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientId, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = generalLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            // Comportamiento obligatorio ante rechazos (HTTP 429 + Formato de Error + Logging)[cite: 1]
            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var endpoint = context.HttpContext.Request.Path;
                var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();

                // Registrar en el log[cite: 1]
                logger.LogWarning("Rate limit excedido en la ruta {Endpoint} desde la IP {ClientIp}", endpoint, clientIp);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                // Formato de error estándar requerido por el TPI[cite: 1]
                var errorResponse = new
                {
                    errorCode = "TOO_MANY_REQUESTS",
                    message = "Se ha superado el límite de solicitudes permitidas."
                };

                await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
            };
        });

        return services;
    }
}
