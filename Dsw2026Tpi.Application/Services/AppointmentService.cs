using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using System.Linq;
using static Dsw2026Tpi.Application.Dtos.PatientDto;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }
    public async Task<AppointmentDto.Response> CreateAppointment(AppointmentDto.Request appointmentDto)
    {
        var Doctor = await _persistence.GetById<Doctor>(appointmentDto.DoctorId);
        if (Doctor == null) throw new EntityNotFoundException("Doctor Not Found");

        var availability = await _persistence.GetById<Availability>(appointmentDto.availability);
        if (availability == null) throw new EntityNotFoundException("Availability Not Found");

        var patients = await _persistence.GetFiltered<Patient>(p => p.DNI == appointmentDto.Patient.Dni);
        if (patients == null || !patients.Any()) throw new EntityNotFoundException("Patient Not Found");
        var patient = patients.FirstOrDefault();
    

        var slot = await _persistence.GetFiltered<Turn>(t => t.AvailabilityId == availability.Id && t.Status == TurnStatus.NO_SHOW);
        if (slot == null || !slot.Any()) throw new EntityNotFoundException("Slot Not Available");
        var turn = new Turn();
        foreach (var s in slot)
        {
            if (s.Status != TurnStatus.NO_SHOW)
            {
                throw new EntityNotFoundException("Slot Not Available");
            }
            turn = slot.FirstOrDefault();
        }

        var appointment = new Appointment(patient.Id, availability.Date);
        turn.Status = TurnStatus.BOOKED;

        await _persistence.Add(appointment);
        await _persistence.Update(turn);

        return new AppointmentDto.Response(patient.Id, availability.Date);
    }
}

