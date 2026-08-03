using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Appointment : EntityBase
    {
        public DateOnly DateOfService { get; init; }
        public DateOnly? CancellationDate { get; set; }
        public Guid PatientId { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;
        public Guid TurnId { get; set; }

        // Propiedades de navegación (esenciales para Entity Framework)
        public Patient Patient { get; set; } = null!;
        public Turn Turn { get; set; } = null!;

        // Constructor requerido por EF Core
        protected Appointment() { }

        public Appointment(Guid patientId, Guid turnId, DateOnly dateOfService)
        {
            PatientId = patientId;
            TurnId = turnId;
            DateOfService = dateOfService;
        }
    }
}
