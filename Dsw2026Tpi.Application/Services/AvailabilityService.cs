using System;
using System.Globalization;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private const int SlotDurationMinutes = 30;

    private readonly IPersistence _persistence;

    private static readonly Dictionary<string, DayOfWeek> DayNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["LUNES"] = DayOfWeek.Monday,
            ["MARTES"] = DayOfWeek.Tuesday,
            ["MIERCOLES"] = DayOfWeek.Wednesday,
            ["JUEVES"] = DayOfWeek.Thursday,
            ["VIERNES"] = DayOfWeek.Friday,
            ["SABADO"] = DayOfWeek.Saturday,
            ["DOMINGO"] = DayOfWeek.Sunday
        };

    public AvailabilityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<IReadOnlyCollection<AvailabilityModel.Response>>
        GetByDoctor(Guid doctorId)
    {
        await EnsureDoctorExists(doctorId);

        var (monthStart, monthEnd) = GetCurrentMonthBounds();

        var availabilities =
            (await _persistence.GetFiltered<Availability>(
                availability =>
                    availability.DoctorId == doctorId &&
                    availability.Date >= monthStart &&
                    availability.Date <= monthEnd))
            ?.OrderBy(availability => availability.Date)
            .ThenBy(availability => availability.StartTime)
            .ToList() ?? [];

        if (availabilities.Count == 0)
        {
            return [];
        }

        return BuildScheduleResponse(availabilities);
    }

    public async Task Create(AvailabilityModel.Request request)
    {
        var ranges = await ValidateRequest(request);
        var (monthStart, monthEnd) = GetCurrentMonthBounds();

        var currentAvailabilities =
            (await GetCurrentMonthAvailabilities(
                request.DoctorId,
                monthStart,
                monthEnd))
            .ToList();

        if (currentAvailabilities.Count > 0)
        {
            throw new ConflictException(
                "AVAILABILITY_ALREADY_EXISTS",
                "La disponibilidad del mes ya fue cargada. Utilizá la actualización para modificarla.");
        }

        // 1. Creamos las disponibilidades base (rangos maestros)
        var newAvailabilities = BuildAvailabilityRanges(
            request.DoctorId,
            ranges,
            monthStart,
            monthEnd);

        if (newAvailabilities.Count == 0)
        {
            throw new ValidationException(
                "Los horarios indicados no generan disponibilidad para el mes actual",
                "AVAILABILITY_NO_FUTURE_SLOTS");
        }

        await _persistence.AddRange(newAvailabilities);

        // ==========================================
        // 📍 AQUÍ ES DONDE VA EL CÓDIGO DE CONTROL:
        // ==========================================
        var availabilityIds = newAvailabilities.Select(a => a.Id).ToList();
        var existingTurnsForDoctor = await _persistence.GetFiltered<Turn>(t => availabilityIds.Contains(t.AvailabilityId));
        var existingTimes = existingTurnsForDoctor?.Select(t => t.StartTime).ToHashSet() ?? new HashSet<DateTime>();

        var allTurns = new List<Turn>();
        foreach (var av in newAvailabilities)
        {
            var turnsForAv = BuildTurnsForAvailability(av.Id, av.Date, av.StartTime, av.EndTime);

            foreach (var turn in turnsForAv)
            {
                if (!existingTimes.Contains(turn.StartTime))
                {
                    allTurns.Add(turn);
                    existingTimes.Add(turn.StartTime);
                }
            }
        }

        if (allTurns.Count > 0)
        {
            await _persistence.AddRange(allTurns);
        }
        // ==========================================
    }

    public async Task Update(AvailabilityModel.Request request)
    {
        var ranges = await ValidateRequest(request);
        var (monthStart, monthEnd) = GetCurrentMonthBounds();

        var currentAvailabilities =
            (await GetCurrentMonthAvailabilities(
                request.DoctorId,
                monthStart,
                monthEnd))
            .ToList();

        if (currentAvailabilities.Count == 0)
        {
            throw new EntityNotFoundException(
                "disponibilidad mensual");
        }

        // Verificamos si algún turno de los actuales ya fue reservado
        var availabilityIds = currentAvailabilities.Select(a => a.Id).ToList();
        var existingTurns = await _persistence.GetFiltered<Turn>(t => availabilityIds.Contains(t.AvailabilityId));

        if (existingTurns != null && existingTurns.Any(t => t.Status == TurnStatus.BOOKED))
        {
            throw new ConflictException(
                "AVAILABILITY_HAS_BOOKED_SLOTS",
                "No se puede reemplazar la disponibilidad porque existen turnos reservados");
        }

        // Generamos los nuevos rangos maestros de disponibilidad
        var newAvailabilities = BuildAvailabilityRanges(
            request.DoctorId,
            ranges,
            monthStart,
            monthEnd);

        if (newAvailabilities.Count == 0)
        {
            throw new ValidationException(
                "Los horarios indicados no generan disponibilidad para el mes actual",
                "AVAILABILITY_NO_FUTURE_SLOTS");
        }

        // Borramos los turnos viejos usando Delete
        if (existingTurns != null && existingTurns.Any())
        {
            foreach (var oldTurn in existingTurns)
            {
                await _persistence.Delete(oldTurn);
            }
        }

        // Reemplazamos el rango de disponibilidades antiguas por las nuevas
        await _persistence.ReplaceRange(
            currentAvailabilities,
            newAvailabilities);

        // ==========================================
        // 📍 GENERACIÓN DE NUEVOS TURNOS SIN DUPLICADOS:
        // ==========================================
        var newAvailabilityIds = newAvailabilities.Select(a => a.Id).ToList();
        var existingTimes = new HashSet<DateTime>();

        var allNewTurns = new List<Turn>();
        foreach (var av in newAvailabilities)
        {
            var turnsForAv = BuildTurnsForAvailability(av.Id, av.Date, av.StartTime, av.EndTime);

            foreach (var turn in turnsForAv)
            {
                if (!existingTimes.Contains(turn.StartTime))
                {
                    allNewTurns.Add(turn);
                    existingTimes.Add(turn.StartTime);
                }
            }
        }

        if (allNewTurns.Count > 0)
        {
            await _persistence.AddRange(allNewTurns);
        }
        // ==========================================
    }

    private async Task<List<ParsedRange>> ValidateRequest(
        AvailabilityModel.Request request)
    {
        if (request.DoctorId == Guid.Empty)
        {
            throw new ValidationException(
                "El identificador del médico es obligatorio!!",
                "DOCTOR_ID_REQUIRED");
        }

        await EnsureDoctorExists(request.DoctorId);

        if (request.Days is null || request.Days.Count == 0)
        {
            throw new ValidationException(
                "Debe indicar al menos un día de atención",
                "AVAILABILITY_DAYS_REQUIRED");
        }

        var ranges = request.Days
            .Select(ParseRange)
            .ToList();

        ValidateOverlappingRanges(ranges);

        return ranges;
    }

    private static ParsedRange ParseRange(
        AvailabilityModel.DayRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Day))
        {
            throw new ValidationException(
                "El día de atención es obligatorio",
                "AVAILABILITY_DAY_REQUIRED");
        }

        var normalizedDay = NormalizeDayName(request.Day);

        if (!DayNames.TryGetValue(
                normalizedDay,
                out var dayOfWeek))
        {
            throw new ValidationException(
                $"El día '{request.Day}' no es válido",
                "AVAILABILITY_INVALID_DAY");
        }

        if (!TimeOnly.TryParseExact(
                request.StartTime,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var startTime))
        {
            throw new ValidationException(
                $"La hora de inicio '{request.StartTime}' debe tener el formato HH:mm",
                "AVAILABILITY_INVALID_START_TIME");
        }

        if (!TimeOnly.TryParseExact(
                request.EndTime,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var endTime))
        {
            throw new ValidationException(
                $"La hora de finalización '{request.EndTime}' debe tener el formato HH:mm",
                "AVAILABILITY_INVALID_END_TIME");
        }

        if (startTime >= endTime)
        {
            throw new ValidationException(
                "La hora de inicio debe ser anterior a la hora de finalización",
                "AVAILABILITY_INVALID_TIME_RANGE");
        }

        if (startTime.Minute % SlotDurationMinutes != 0 ||
            endTime.Minute % SlotDurationMinutes != 0)
        {
            throw new ValidationException(
                "Los horarios deben comenzar y finalizar en intervalos de 30 minutos",
                "AVAILABILITY_INVALID_INTERVAL");
        }

        var duration = endTime - startTime;

        if (duration.Ticks %
            TimeSpan.FromMinutes(SlotDurationMinutes).Ticks != 0)
        {
            throw new ValidationException(
                "El rango debe poder dividirse en bloques de 30 minutos",
                "AVAILABILITY_INVALID_DURATION");
        }

        return new ParsedRange(
            dayOfWeek,
            startTime,
            endTime);
    }

    private static void ValidateOverlappingRanges(
        IEnumerable<ParsedRange> ranges)
    {
        foreach (var dayGroup in ranges.GroupBy(
                     range => range.DayOfWeek))
        {
            var orderedRanges = dayGroup
                .OrderBy(range => range.StartTime)
                .ToList();

            for (var index = 1;
                 index < orderedRanges.Count;
                 index++)
            {
                var previous = orderedRanges[index - 1];
                var current = orderedRanges[index];

                if (current.StartTime < previous.EndTime)
                {
                    throw new ValidationException(
                        $"Existen Horarios Superpuestos para {GetSpanishDayName(dayGroup.Key)}",
                        "AVAILABILITY_OVERLAPPING_RANGES");
                }
            }
        }
    }

    private static List<Availability> BuildSlots(
        Guid doctorId,
        IEnumerable<ParsedRange> ranges,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        var availabilities = new List<Availability>();
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        var currentDate = today > monthStart
            ? today
            : monthStart;

        var rangesByDay = ranges
            .GroupBy(range => range.DayOfWeek)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        while (currentDate <= monthEnd)
        {
            if (rangesByDay.TryGetValue(
                    currentDate.DayOfWeek,
                    out var rangesForDay))
            {
                foreach (var range in rangesForDay)
                {
                    var slotStart = range.StartTime;

                    while (slotStart < range.EndTime)
                    {
                        var slotEnd =
                            slotStart.AddMinutes(
                                SlotDurationMinutes);

                        var slotDateTime =
                            currentDate.ToDateTime(slotStart);

                        if (slotDateTime > now)
                        {
                            availabilities.Add(
                                new Availability(
                                    doctorId,
                                    currentDate,
                                    slotStart,
                                    slotEnd));
                        }

                        slotStart = slotEnd;
                    }
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return availabilities;
    }

    private static IReadOnlyCollection<AvailabilityModel.Response>
        BuildScheduleResponse(
            IEnumerable<Availability> availabilities)
    {
        var responses =
            new List<AvailabilityModel.Response>();

        var slotsByDay = availabilities
            .Select(availability => new
            {
                Day = availability.Date.DayOfWeek,
                availability.StartTime,
                availability.EndTime
            })
            .Distinct()
            .GroupBy(slot => slot.Day)
            .OrderBy(group => GetDayOrder(group.Key));

        foreach (var dayGroup in slotsByDay)
        {
            var orderedSlots = dayGroup
                .OrderBy(slot => slot.StartTime)
                .ToList();

            if (orderedSlots.Count == 0)
            {
                continue;
            }

            var rangeStart = orderedSlots[0].StartTime;
            var rangeEnd = orderedSlots[0].EndTime;

            foreach (var slot in orderedSlots.Skip(1))
            {
                if (slot.StartTime == rangeEnd)
                {
                    rangeEnd = slot.EndTime;
                    continue;
                }

                responses.Add(CreateResponse(
                    dayGroup.Key,
                    rangeStart,
                    rangeEnd));

                rangeStart = slot.StartTime;
                rangeEnd = slot.EndTime;
            }

            responses.Add(CreateResponse(
                dayGroup.Key,
                rangeStart,
                rangeEnd));
        }

        return responses;
    }

    private async Task EnsureDoctorExists(Guid doctorId)
    {
        var doctor =
            await _persistence.GetById<Doctor>(doctorId);

        if (doctor is null)
        {
            throw new EntityNotFoundException("médico");
        }
    }

    private async Task<IEnumerable<Availability>>
        GetCurrentMonthAvailabilities(
            Guid doctorId,
            DateOnly monthStart,
            DateOnly monthEnd)
    {
        return await _persistence.GetFiltered<Availability>(
                   availability =>
                       availability.DoctorId == doctorId &&
                       availability.Date >= monthStart &&
                       availability.Date <= monthEnd)
               ?? [];
    }

    private static void EnsureSlotsWereGenerated(
        IReadOnlyCollection<Availability> availabilities)
    {
        if (availabilities.Count == 0)
        {
            throw new ValidationException(
                "Los horarios indicados no generan slots futuros para el mes actual",
                "AVAILABILITY_NO_FUTURE_SLOTS");
        }
    }

    private static (
        DateOnly MonthStart,
        DateOnly MonthEnd)
        GetCurrentMonthBounds()
    {
        var today =
            DateOnly.FromDateTime(DateTime.Today);

        var monthStart =
            new DateOnly(today.Year, today.Month, 1);

        var monthEnd =
            monthStart.AddMonths(1).AddDays(-1);

        return (monthStart, monthEnd);
    }

    private static AvailabilityModel.Response CreateResponse(
        DayOfWeek day,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return new AvailabilityModel.Response(
            GetSpanishDayName(day),
            startTime.ToString(
                "HH:mm",
                CultureInfo.InvariantCulture),
            endTime.ToString(
                "HH:mm",
                CultureInfo.InvariantCulture));
    }

    private static string NormalizeDayName(string day)
    {
        var normalized = day
            .Trim()
            .Normalize(NormalizationForm.FormD);

        var characters = normalized
            .Where(character =>
                CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(characters)
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();
    }

    private static string GetSpanishDayName(
        DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "LUNES",
            DayOfWeek.Tuesday => "MARTES",
            DayOfWeek.Wednesday => "MIÉRCOLES",
            DayOfWeek.Thursday => "JUEVES",
            DayOfWeek.Friday => "VIERNES",
            DayOfWeek.Saturday => "SÁBADO",
            DayOfWeek.Sunday => "DOMINGO",
            _ => throw new ArgumentOutOfRangeException( nameof(day))
        };
    }

    private static int GetDayOrder(DayOfWeek day)
    {
        return day == DayOfWeek.Sunday
            ? 7
            : (int)day;
    }

    private sealed record ParsedRange( DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

    private static List<Availability> BuildAvailabilityRanges(
        Guid doctorId,
        IEnumerable<ParsedRange> ranges,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        var availabilities = new List<Availability>();
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        var currentDate = today > monthStart
            ? today
            : monthStart;

        var rangesByDay = ranges
            .GroupBy(range => range.DayOfWeek)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        while (currentDate <= monthEnd)
        {
            if (rangesByDay.TryGetValue(
                    currentDate.DayOfWeek,
                    out var rangesForDay))
            {
                foreach (var range in rangesForDay)
                {
                    // Guardamos la franja completa o por tramos que haya definido el médico
                    availabilities.Add(
                        new Availability(
                            doctorId,
                            currentDate,
                            range.StartTime,
                            range.EndTime));
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return availabilities;
    }

    private static List<Turn> BuildTurnsForAvailability(
        Guid availabilityId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        var turns = new List<Turn>();
        var now = DateTime.Now;
        var slotStart = startTime;

        while (slotStart < endTime)
        {
            var slotEnd = slotStart.AddMinutes(SlotDurationMinutes);
            var startDateTime = date.ToDateTime(slotStart);
            var endDateTime = date.ToDateTime(slotEnd);

            // Solo creamos turnos que sean a futuro respecto al momento actual
            if (startDateTime > now)
            {
                turns.Add(
                    new Turn(
                        availabilityId,
                        date,
                        startDateTime,
                        endDateTime));
            }

            slotStart = slotEnd;
        }

        return turns;
    }
}