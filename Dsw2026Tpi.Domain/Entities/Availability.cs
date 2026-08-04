using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Availability : EntityBase
{
    public Guid DoctorId { get; private set; }

    public Doctor Doctor { get; private set; } = null!;

    public DateOnly Date { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public AvailabilityStatus Status { get; private set; }
    public List<Turn> Turns { get; private set; } = new List<Turn>();

    #region Constructor for EF

#pragma warning disable CS8618
    public Availability()
    {
    }
#pragma warning restore CS8618

    #endregion

    public Availability(
            Guid doctorId,
            DateOnly date,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? id = null) : base(id)
    {
        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del médico es obligatorio.",
                nameof(doctorId));
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "La hora de finalización debe ser posterior a la hora de inicio.");
        }

        // ❌ ELIMINAMOS O COMENTAMOS ESTA RESTRICCIÓN PARA QUE ACEPTE RANGOS LARGOS:
        /*
        if (endTime - startTime != TimeSpan.FromMinutes(30))
        {
            throw new ArgumentException("Cada disponibilidad debe representar exactamente 30 minutos.");
        }
        */

        DoctorId = doctorId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Status = AvailabilityStatus.Available; // O el estado inicial de la franja

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Book()
    {
        if (Status != AvailabilityStatus.Available)
        {
            throw new InvalidOperationException(
                "Solo puede reservarse una disponibilidad que se encuentre disponible.");
        }

        Status = AvailabilityStatus.Booked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        if (Status != AvailabilityStatus.Booked)
        {
            throw new InvalidOperationException(
                "Solo puede liberarse una disponibilidad reservada.");
        }

        Status = AvailabilityStatus.Available;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        if (Status == AvailabilityStatus.Booked)
        {
            throw new InvalidOperationException(
                "No puede bloquearse una disponibilidad que ya fue reservada.");
        }

        Status = AvailabilityStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unblock()
    {
        if (Status != AvailabilityStatus.Blocked)
        {
            throw new InvalidOperationException(
                "Solo puede desbloquearse una disponibilidad bloqueada.");
        }

        Status = AvailabilityStatus.Available;
        UpdatedAt = DateTime.UtcNow;
    }
}
