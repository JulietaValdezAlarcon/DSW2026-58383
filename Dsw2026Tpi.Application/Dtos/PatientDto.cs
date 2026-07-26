using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record PatientDto
{
    // DNI is numeric; use Range to validate number of digits instead of MinLength/MaxLength which are for strings/collections
    public record patientDni([Range(1000000, 9999999999)] int Dni);
}
