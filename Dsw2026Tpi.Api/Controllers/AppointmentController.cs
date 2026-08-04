using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [EnableRateLimiting("AppointmentPolicy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto.Request appointmentDto)
    {
        var result = await _service.CreateAppointment(appointmentDto);
        return Created("", result);
    }

    [HttpGet("patient")]
    [EnableRateLimiting("GeneralPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentByDni([FromQuery] int dni)
    {
        var result = await _service.GetAppointmentByDni(dni);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("GeneralPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAppointment([FromRoute] Guid id)
    {
        await _service.DeleteAppointment(id);
        return Ok("ok");
    }

    [HttpGet]
    [EnableRateLimiting("GeneralPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateOnly? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        if (!date.HasValue)
        {
            throw new ArgumentException("La fecha es obligatoria para este endpoint.");
        }

        var result = await _service.GetTurnsByDay(date.Value, pageSize, pageIndex);
        return Ok(result);
    }

    [HttpGet("search")]
    [EnableRateLimiting("GeneralPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAppointments([FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, [FromQuery] int? dni, [FromQuery] DateOnly? date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var result = await _service.SearchTurns(specialtyId, doctorId, dni, date, pageSize, pageIndex);
        return Ok(result);
    }
}