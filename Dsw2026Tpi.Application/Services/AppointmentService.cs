using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<AppointmentDto.TurnRow>> GetAvailableTurnsByDay(DateOnly date, int pageSize = 10, int pageIndex = 0)
    {
        var availabilities = await _persistence.GetFiltered<Availability>(a => a.Date == date);
        if (availabilities == null || !availabilities.Any())
        {
            return Pagination<AppointmentDto.TurnRow>.Empty;
        }

        var rows = new List<AppointmentDto.TurnRow>();

        foreach (var availability in availabilities)
        {
            var doctor = await _persistence.GetById<Doctor>(availability.DoctorId, "Speciality");
            var specialityName = doctor?.Speciality?.Name ?? string.Empty;
            var doctorName = doctor?.Name ?? string.Empty;

            var turns = await _persistence.GetFiltered<Turn>(t => t.AvailabilityId == availability.Id && t.Status == TurnStatus.NO_SHOW);
            if (turns == null) continue;

            foreach (var turn in turns)
            {
                rows.Add(new AppointmentDto.TurnRow(turn.Id, specialityName, doctorName, TimeOnly.FromDateTime(turn.StartTime)));
            }
        }

        var ordered = rows.OrderBy(r => r.AvailableTime).ToList();
        var total = ordered.Count;

        if (total == 0)
        {
            return Pagination<AppointmentDto.TurnRow>.Empty;
        }

        pageSize = Math.Max(1, pageSize);
        pageIndex = Math.Max(0, pageIndex);

        var pageData = ordered.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        return new Pagination<AppointmentDto.TurnRow>(pageSize, pageIndex, total, pageData);
    }
    public async Task<AppointmentDto.Response> CreateAppointment(AppointmentDto.Request appointmentDto)
    {
        var Doctor = await _persistence.GetById<Doctor>(appointmentDto.DoctorId);
        if (Doctor == null) throw new EntityNotFoundException("Doctor Not Found");

        // Verify patient and selected turn
        var patient = await _persistence.First<Patient>(p => p.DNI == appointmentDto.Patient.Dni);
        if (patient == null) throw new EntityNotFoundException("Patient Not Found");
        var turn = await _persistence.GetById<Turn>(appointmentDto.TurnId);
        if (turn == null) throw new EntityNotFoundException("Turn Not Found");
        if (turn.Status != TurnStatus.NO_SHOW) throw new ConflictException("APPOINTMENT_CONFLICT", "Slot not available");

        var availability = await _persistence.GetById<Availability>(turn.AvailabilityId);
        if (availability == null) throw new EntityNotFoundException("Availability Not Found");

        // Assign appointment and link with turn inside a transaction to avoid double-booking
        var appointment = new Appointment(patient.Id, availability.Date);
        appointment.TurnId = turn.Id;
        turn.AppointmentId = appointment.Id;
        turn.Status = TurnStatus.BOOKED;

        try
        {
            await _persistence.ExecuteInTransaction(async () =>
            {
                await _persistence.Add(appointment);
                await _persistence.Update(turn);
            });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // someone else modified the turn concurrently -> conflict
            throw new ConflictException("APPOINTMENT_CONFLICT", "Slot already booked");
        }

        return new AppointmentDto.Response(patient.Id, availability.Date);
    }

    public async Task<List<AppointmentDto.Response>> GetAppointmentByDni(int dni)
    {
        var patient = await _persistence.First<Patient>(p => p.DNI == dni);
        if (patient == null) throw new EntityNotFoundException("Patient Not Found");

        var appointments = patient.appointments ?? new List<Appointment>();
        var turns = new List<Turn>();
        foreach (var appointment in appointments)
        {
           var turn = await _persistence.GetById<Turn>(appointment.TurnId);
           if (turn != null) turns.Add(turn);
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
        if (appointment == null) throw new EntityNotFoundException("Appointment Not Found");

        var turn = await _persistence.GetById<Turn>(appointment.TurnId);
        switch (appointment.Status)
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
    }

    public async Task<Pagination<AppointmentDto.TurnRow>> GetTurnsByDay(DateOnly date, int pageSize = 10, int pageIndex = 0)
    {

        var availabilities = await _persistence.GetFiltered<Availability>(a => a.Date == date);
        if (availabilities == null || !availabilities.Any())
        {
            return Pagination<AppointmentDto.TurnRow>.Empty;
        }

        var rows = new List<AppointmentDto.TurnRow>();

        foreach (var availability in availabilities)
        {
            var doctor = await _persistence.GetById<Doctor>(availability.DoctorId, "Speciality");
            var specialityName = doctor?.Speciality?.Name ?? string.Empty;
            var doctorName = doctor?.Name ?? string.Empty;

            var turns = await _persistence.GetFiltered<Turn>(t => t.AvailabilityId == availability.Id);
            if (turns == null) continue;

            foreach (var turn in turns)
            { 
                rows.Add(new AppointmentDto.TurnRow(turn.Id, specialityName, doctorName, TimeOnly.FromDateTime(turn.StartTime)));
            }
        }

        var ordered = rows.OrderBy(r => r.AvailableTime).ToList();
        var total = ordered.Count;

        if (total == 0)
        {
            return Pagination<AppointmentDto.TurnRow>.Empty;
        }

        pageSize = Math.Max(1, pageSize);
        pageIndex = Math.Max(0, pageIndex);

        var pageData = ordered.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        return new Pagination<AppointmentDto.TurnRow>(pageSize, pageIndex, total, pageData);
    }

    public async Task<Pagination<AppointmentDto.TurnRow>> SearchTurns(Guid? specialtyId = null, Guid? doctorId = null, int? dni = null, DateOnly? date = null, int pageSize = 10, int pageIndex = 0)
    {
        IEnumerable<Availability>? availabilities;
        if (date.HasValue)
        {
            availabilities = await _persistence.GetFiltered<Availability>(a => a.Date == date.Value);
        }
        else
        {
            availabilities = await _persistence.GetAll<Availability>();
        }

        if (availabilities == null || !availabilities.Any()) return Pagination<AppointmentDto.TurnRow>.Empty;

        var rows = new List<AppointmentDto.TurnRow>();

        HashSet<Guid>? patientAppointmentIds = null;
        if (dni.HasValue)
        {
            var patient = await _persistence.First<Patient>(p => p.DNI == dni.Value);
            if (patient == null) return Pagination<AppointmentDto.TurnRow>.Empty;
            var appointments = await _persistence.GetFiltered<Appointment>(a => a.PatientId == patient.Id);
            patientAppointmentIds = appointments?.Select(a => a.Id).ToHashSet() ?? new HashSet<Guid>();
        }

        foreach (var availability in availabilities)
        {
            if (doctorId.HasValue && availability.DoctorId != doctorId.Value) continue;

            var doctor = await _persistence.GetById<Doctor>(availability.DoctorId, "Speciality");
            if (doctor == null) continue;

            if (specialtyId.HasValue && doctor.SpecialityId != specialtyId.Value) continue;

            var specialityName = doctor.Speciality?.Name ?? string.Empty;
            var doctorName = doctor.Name ?? string.Empty;

            var turns = await _persistence.GetFiltered<Turn>(t => t.AvailabilityId == availability.Id);
            if (turns == null) continue;

            foreach (var turn in turns)
            {
                if (patientAppointmentIds != null)
                {
                    if (!turn.AppointmentId.HasValue) continue;
                    if (!patientAppointmentIds.Contains(turn.AppointmentId.Value)) continue;
                }

                rows.Add(new AppointmentDto.TurnRow(turn.Id, specialityName, doctorName, TimeOnly.FromDateTime(turn.StartTime)));
            }
        }

        var ordered = rows.OrderBy(r => r.AvailableTime).ToList();
        var total = ordered.Count;
        if (total == 0) return Pagination<AppointmentDto.TurnRow>.Empty;

        pageSize = Math.Max(1, pageSize);
        pageIndex = Math.Max(0, pageIndex);

        var pageData = ordered.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        return new Pagination<AppointmentDto.TurnRow>(pageSize, pageIndex, total, pageData);
    }


}

