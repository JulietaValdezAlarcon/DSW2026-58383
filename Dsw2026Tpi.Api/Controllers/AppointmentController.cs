using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("appointments")]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto.Request appointmentDto)
    {
        // No-op: placeholder to keep API signature stable after DTO change
        var result = await _service.CreateAppointment(appointmentDto);
        return Ok(result);
    }

    [HttpGet("by-dni/{dni}")]
    public async Task<IActionResult> GetAppointmentByDni([FromRoute] int dni)
    {
        var result = await _service.GetAppointmentByDni(dni);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment([FromRoute] Guid id)
    {
        // Implement the logic to delete an appointment by its ID
        // For example, you can call a method in the service layer to perform the deletion
        await _service.DeleteAppointment(id);
        return NoContent();
    }

    [HttpGet("by-date/{dateOfService}/{pageSize?}/{pageIndex?}")]
    public async Task<IActionResult> GetAppointmentsByDate([FromRoute] DateOnly dateOfService, int pageSize = 10, int pageIndex = 0)
    {
        var result = await _service.GetTurnsByDay(dateOfService, pageSize, pageIndex);
        return Ok(result);
    }

    [HttpGet("available/by-date/{dateOfService}/{pageSize?}/{pageIndex?}")]
    public async Task<IActionResult> GetAvailableTurnsByDate([FromRoute] DateOnly dateOfService, int pageSize = 10, int pageIndex = 0)
    {
        var result = await _service.GetAvailableTurnsByDay(dateOfService, pageSize, pageIndex);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchAppointments([FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, [FromQuery] int? dni, [FromQuery] DateOnly? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var result = await _service.SearchTurns(specialtyId, doctorId, dni, date, pageSize, pageIndex);
        return Ok(result);
    }
}
