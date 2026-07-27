using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentDto
{
    public record Request (Guid DoctorId, [Required]Guid availability,PatientDto.patientDni Patient, [MinLength(5)] [Required] string reason );
    public record Response (Guid PatientId, DateOnly DateOfService);
}
