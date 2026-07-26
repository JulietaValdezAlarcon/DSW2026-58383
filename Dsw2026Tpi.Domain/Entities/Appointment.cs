using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Appointment : EntityBase
{
    public DateOnly DateOfService { get; set; }
    public DateOnly? CancellationDate { get; set; }
    public Guid PatientId { get; set; }
    public AppointmentStatus Status { get; set; }

}
