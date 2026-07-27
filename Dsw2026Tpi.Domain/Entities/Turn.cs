using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Turn : EntityBase
{
    public DateOnly Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? AppointmentId { get; set; }
    public TurnStatus Status { get; set; } = TurnStatus.NO_SHOW;
    public Guid AvailabilityId { get; set; }

    // RowVersion for optimistic concurrency control
    public byte[]? RowVersion { get; set; }
}
