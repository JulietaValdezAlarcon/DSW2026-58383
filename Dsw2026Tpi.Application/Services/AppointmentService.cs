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

        var patient = await _persistence.First<Patient>(p => p.DNI == appointmentDto.Patient.Dni);
        if (patient == null) throw new EntityNotFoundException("Patient Not Found");
    
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

    public async Task<List<AppointmentDto.Response>> GetAppointmentByDni(int dni)
    {
        var patient = await _persistence.First<Patient>(p => p.DNI == dni);
        if (patient == null) throw new EntityNotFoundException("Patient Not Found");

        var appointments = patient.appointments ?? new List<Appointment>();
        List<Turn>? turns = new List<Turn>();
        foreach (var appointment in appointments)
        {
           var turn = await _persistence.GetById<Turn>(appointment.TurnId);
           turns.Add(turn);
        }
        return appointments
            .Where(a => a.Status == AppointmentStatus.Confirmed)
            .Where(a => turns.Any(t => t.Id == a.TurnId && t.Status == TurnStatus.BOOKED))
            .Select(a => new AppointmentDto.Response(patient.Id, a.DateOfService))
            .ToList();
    }

    public async Task DeleteAppointment(Guid id)
    {
        var appointment = await _persistence.GetById<Appointment>(id);
        var turn = await _persistence.GetById<Turn>(appointment.TurnId);
        switch (appointment?.Status)
        {
            case AppointmentStatus.Confirmed:
                switch (turn?.Status)
                {
                    case TurnStatus.BOOKED:
                        turn.Status = TurnStatus.CANCELLED;
                        await _persistence.Update(turn);
                        break;
                    case TurnStatus.NO_SHOW:
                        throw new InvalidOperationException("Turn is already marked as no-show.");
                    case TurnStatus.CANCELLED:
                        throw new InvalidOperationException("Turn is already cancelled.");
                    case TurnStatus.ATTENDED:
                        throw new InvalidOperationException("Cannot delete an appointment for an attended turn.");
                    default:
                        throw new EntityNotFoundException("Turn Not Found");
                }
                appointment.Status = AppointmentStatus.Cancelled;
                await _persistence.Update(appointment);
                break;
            case AppointmentStatus.Cancelled:
                throw new InvalidOperationException("Appointment is already cancelled.");
            case AppointmentStatus.Completed:
                throw new InvalidOperationException("Cannot delete a completed appointment.");
            default:
                throw new EntityNotFoundException("Appointment Not Found");
        }
        
        await _persistence.Update(appointment);
    }
}

