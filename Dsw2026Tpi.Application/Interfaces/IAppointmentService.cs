using Dsw2026Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDto.Response> CreateAppointment(AppointmentDto.Request appointmentDto);   
    Task<Dsw2026Tpi.Domain.Entities.Pagination<AppointmentDto.TurnRow>> GetTurnsByDay(DateOnly date, int pageSize = 10, int pageIndex = 0);
    Task<Dsw2026Tpi.Domain.Entities.Pagination<AppointmentDto.TurnRow>> GetAvailableTurnsByDay(DateOnly date, int pageSize = 10, int pageIndex = 0);
    Task<Dsw2026Tpi.Domain.Entities.Pagination<AppointmentDto.TurnRow>> SearchTurns(Guid? specialtyId = null, Guid? doctorId = null, int? dni = null, DateOnly? date = null, int pageSize = 10, int pageIndex = 0);
    Task<List<AppointmentDto.Response>> GetAppointmentByDni(int dni);
    Task DeleteAppointment(Guid id);

}
