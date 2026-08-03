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

        var availabilityIds = availabilities.Select(a => a.Id).ToList();
        var doctorIds = availabilities.Select(a => a.DoctorId).Distinct().ToList();

        // 🚀 Optimización: Traer todos los turnos y doctores de una sola vez
        var turns = await _persistence.GetFiltered<Turn>(t => availabilityIds.Contains(t.AvailabilityId) && t.Status == TurnStatus.NO_SHOW);
        var doctors = await _persistence.GetFiltered<Doctor>(d => doctorIds.Contains(d.Id), "Speciality");

        var doctorDict = doctors?.ToDictionary(d => d.Id) ?? new Dictionary<Guid, Doctor>();
        var rows = new List<AppointmentDto.TurnRow>();

        if (turns != null)
        {
            foreach (var turn in turns)
            {
                var availability = availabilities.FirstOrDefault(a => a.Id == turn.AvailabilityId);
                if (availability == null) continue;

                doctorDict.TryGetValue(availability.DoctorId, out var doctor);
                var specialityName = doctor?.Speciality?.Name ?? string.Empty;
                var doctorName = doctor?.Name ?? string.Empty;

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

    public async Task<AppointmentDto.Response> CreateAppointment(AppointmentDto.Request appointmentDto)
    {
        var doctor = await _persistence.GetById<Doctor>(appointmentDto.DoctorId);
        if (doctor == null) throw new EntityNotFoundException("Doctor Not Found");

        var patient = await _persistence.First<Patient>(p => p.DNI == appointmentDto.Patient.Dni);
        if (patient == null) throw new EntityNotFoundException("Patient Not Found");

        var turn = await _persistence.GetById<Turn>(appointmentDto.TurnId);
        if (turn == null) throw new EntityNotFoundException("Turn Not Found");
        if (turn.Status != TurnStatus.NO_SHOW) throw new ConflictException("APPOINTMENT_CONFLICT", "Slot not available");

        var availability = await _persistence.GetById<Availability>(turn.AvailabilityId);
        if (availability == null) throw new EntityNotFoundException("Availability Not Found");

        var appointment = new Appointment(patient.Id, turn.Id, availability.Date);
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
            throw new ConflictException("APPOINTMENT_CONFLICT", "Slot already booked");
        }

        return new AppointmentDto.Response(patient.Id, availability.Date);
    }

    public async Task<List<AppointmentDto.Response>> GetAppointmentByDni(int dni)
    {
        var patient = await _persistence.First<Patient>(p => p.DNI == dni, "Appointments");
        if (patient == null) throw new EntityNotFoundException("Patient Not Found");

        var appointments = patient.Appointments ?? new List<Appointment>();
        if (!appointments.Any()) return new List<AppointmentDto.Response>();

        var turnIds = appointments.Select(a => a.TurnId).ToList();
        var turns = await _persistence.GetFiltered<Turn>(t => turnIds.Contains(t.Id));

        return appointments
            .Where(a => a.Status == AppointmentStatus.Confirmed)
            .Where(a => turns != null && turns.Any(t => t.Id == a.TurnId && t.Status == TurnStatus.BOOKED))
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

        var availabilityIds = availabilities.Select(a => a.Id).ToList();
        var doctorIds = availabilities.Select(a => a.DoctorId).Distinct().ToList();

        var turns = await _persistence.GetFiltered<Turn>(t => availabilityIds.Contains(t.AvailabilityId));
        var doctors = await _persistence.GetFiltered<Doctor>(d => doctorIds.Contains(d.Id), "Speciality");

        var doctorDict = doctors?.ToDictionary(d => d.Id) ?? new Dictionary<Guid, Doctor>();
        var rows = new List<AppointmentDto.TurnRow>();

        if (turns != null)
        {
            foreach (var turn in turns)
            {
                var availability = availabilities.FirstOrDefault(a => a.Id == turn.AvailabilityId);
                if (availability == null) continue;

                doctorDict.TryGetValue(availability.DoctorId, out var doctor);
                var specialityName = doctor?.Speciality?.Name ?? string.Empty;
                var doctorName = doctor?.Name ?? string.Empty;

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

        // Filtrar por doctor si se pasó por parámetro
        if (doctorId.HasValue)
        {
            availabilities = availabilities.Where(a => a.DoctorId == doctorId.Value).ToList();
        }

        var availabilityIds = availabilities.Select(a => a.Id).ToList();
        var doctorIds = availabilities.Select(a => a.DoctorId).Distinct().ToList();

        var doctors = await _persistence.GetFiltered<Doctor>(d => doctorIds.Contains(d.Id), "Speciality");

        // Filtrar por especialidad si se pasó por parámetro
        if (specialtyId.HasValue && doctors != null)
        {
            var validDoctorIds = doctors.Where(d => d.SpecialityId == specialtyId.Value).Select(d => d.Id).ToHashSet();
            availabilities = availabilities.Where(a => validDoctorIds.Contains(a.DoctorId)).ToList();
            availabilityIds = availabilities.Select(a => a.Id).ToList();
        }

        if (!availabilities.Any()) return Pagination<AppointmentDto.TurnRow>.Empty;

        HashSet<Guid>? patientAppointmentIds = null;
        if (dni.HasValue)
        {
            var patient = await _persistence.First<Patient>(p => p.DNI == dni.Value);
            if (patient == null) return Pagination<AppointmentDto.TurnRow>.Empty;
            var appointments = await _persistence.GetFiltered<Appointment>(a => a.PatientId == patient.Id);
            patientAppointmentIds = appointments?.Select(a => a.Id).ToHashSet() ?? new HashSet<Guid>();
        }

        var turns = await _persistence.GetFiltered<Turn>(t => availabilityIds.Contains(t.AvailabilityId));
        var doctorDict = doctors?.ToDictionary(d => d.Id) ?? new Dictionary<Guid, Doctor>();
        var rows = new List<AppointmentDto.TurnRow>();

        if (turns != null)
        {
            foreach (var turn in turns)
            {
                if (patientAppointmentIds != null)
                {
                    if (!turn.AppointmentId.HasValue) continue;
                    if (!patientAppointmentIds.Contains(turn.AppointmentId.Value)) continue;
                }

                var availability = availabilities.FirstOrDefault(a => a.Id == turn.AvailabilityId);
                if (availability == null) continue;

                doctorDict.TryGetValue(availability.DoctorId, out var doctor);
                if (doctor == null) continue;

                var specialityName = doctor.Speciality?.Name ?? string.Empty;
                var doctorName = doctor.Name ?? string.Empty;

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

