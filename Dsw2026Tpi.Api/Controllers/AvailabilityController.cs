using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("availabilities")]
[Authorize]
public class AvailabilityController : AppController
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(
        IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    //Nota: Los símbolos ~/ hacen que ASP.NET Core utilice las rutas exactas indicadas
    [HttpGet("~/api/doctors/{doctorId:guid}/availabilities")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AvailabilityModel.Response>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<IReadOnlyCollection<AvailabilityModel.Response>>>
        GetByDoctor(Guid doctorId)
    {
        var availability =
            await _availabilityService.GetByDoctor(doctorId);

        return Ok(availability);
    }

    [HttpPost("~/api/availabilities")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] AvailabilityModel.Request request)
    {
        await _availabilityService.Create(request);

        return CreatedAtAction(
            nameof(GetByDoctor),
            new { doctorId = request.DoctorId },
            value: null);
    }

    [HttpPut("~/api/availabilities")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromBody] AvailabilityModel.Request request)
    {
        await _availabilityService.Update(request);

        return NoContent();
    }
}