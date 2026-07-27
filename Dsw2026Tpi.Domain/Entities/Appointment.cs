using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Appointment : EntityBase
{
    public DateOnly DateOfService { get; init; }
    public DateOnly? CancellationDate { get; set; }
    public Guid PatientId { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;
    public Guid TurnId { get; set; }

    public Appointment (Guid patientId, DateOnly dateOfService)
    {
        PatientId = patientId;
        DateOfService = dateOfService;
    }

}
