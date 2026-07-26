using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Turn
{
    public DateOnly Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? AppointmentId { get; set; }
    public TurnStatus Status { get; set; } = TurnStatus.Available;
    public Guid AvailabilityId { get; set; }
}
