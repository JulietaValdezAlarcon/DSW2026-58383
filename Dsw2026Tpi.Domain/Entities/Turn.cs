using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Turn : EntityBase
    {
        public DateOnly Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? AppointmentId { get; set; }
        public TurnStatus Status { get; set; } = TurnStatus.NO_SHOW;
        public Guid AvailabilityId { get; set; }

        // Propiedad de navegación opcional hacia Appointment
        public Appointment? Appointment { get; set; }

        // RowVersion for optimistic concurrency control
        public byte[]? RowVersion { get; set; }

        // Constructor requerido por EF Core
        public Turn() { }

        public Turn(Guid availabilityId, DateOnly date, DateTime startTime, DateTime endTime, Guid? id = null) : base(id)
        {
            AvailabilityId = availabilityId;
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            Status = TurnStatus.NO_SHOW; // O el estado inicial que corresponda en tu lógica
        }
    }
}