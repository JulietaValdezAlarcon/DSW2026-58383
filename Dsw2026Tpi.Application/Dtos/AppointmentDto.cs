using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentDto
{
// Request now expects a specific TurnId chosen by the client to avoid implicit selection
public record Request (Guid DoctorId, [Required] Guid TurnId, PatientDto.patientDni Patient, [MinLength(5)] [Required] string reason );
    public record Response (Guid PatientId, DateOnly DateOfService);

    // Row used for listing turns in a logical table. Includes TurnId so clients can pick a turn to book.
    public record TurnRow(Guid TurnId, string Specialty, string Doctor, TimeOnly AvailableTime);
}
