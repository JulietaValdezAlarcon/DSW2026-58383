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
        var result = await _service.CreateAppointment(appointmentDto);
        return Ok(result);
    }

    [HttpGet("{dni}")]
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
}
