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
    private readonly IAvailabilityService _service;

    public AvailabilityController(IAvailabilityService service)
    {
        _service = service;
    }

    [HttpGet("/api/doctors/{doctorId:guid}/availabilities")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AvailabilityModel.Response>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        var availabilities = await _service.GetByDoctor(doctorId);

        return Ok(availabilities);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] AvailabilityModel.Request request)
    {
        await _service.Create(request);

        return Created(
            $"/api/doctors/{request.DoctorId}/availabilities",
            null);
    }

    [HttpPut]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromBody] AvailabilityModel.Request request)
    {
        await _service.Update(request);

        return NoContent();
    }
}
