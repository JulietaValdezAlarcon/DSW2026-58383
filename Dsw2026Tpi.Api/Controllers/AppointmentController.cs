using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/appointments")] // Asegurar prefijo /api si no lo maneja AppController
[Authorize]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [EnableRateLimiting("AppointmentPolicy")]
    public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto.Request appointmentDto)
    {
        var result = await _service.CreateAppointment(appointmentDto);
        return Created("", result); // Cambiado a 201 Created según buenas prácticas REST
    }

    // Corregido según TPI: GET /api/appointments/patient?dni=number
    [HttpGet("patient")]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> GetAppointmentByDni([FromQuery] int dni)
    {
        var result = await _service.GetAppointmentByDni(dni);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> DeleteAppointment([FromRoute] Guid id)
    {
        await _service.DeleteAppointment(id);
        return Ok("ok"); // Corregido según TPI: Retorna HTTP 200 con el texto "ok"
    }

    // Corregido según TPI: GET /api/appointments?date=YYYY-MM-DD
    [HttpGet]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateOnly? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        if (date.HasValue)
        {
            var result = await _service.GetTurnsByDay(date.Value, pageSize, pageIndex);
            return Ok(result);
        }

        // Si no mandan fecha, podrías derivarlo a la búsqueda general o retornar vacío
        return BadRequest("La fecha es obligatoria para este endpoint.");
    }

    [HttpGet("search")]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> SearchAppointments([FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, [FromQuery] int? dni, [FromQuery] DateOnly? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        // Asegúrate de usar el servicio que devuelve la estructura anidada de Citas (SearchAppointments)
        var result = await _service.SearchTurns(specialtyId, doctorId, dni, date, pageSize, pageIndex);
        return Ok(result);
    }
}