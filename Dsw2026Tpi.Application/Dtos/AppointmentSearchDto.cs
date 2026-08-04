using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public class AppointmentSearchDto
{
    public record Response(
        int PageSize,
        int PageIndex,
        List<AppointmentRow> Data,
        int Total
    );

    public record AppointmentRow(
        Guid AppointmentsId,
        string AppointmentsStatus,
        PatientInfo Patient,
        DoctorInfo Doctor
    );

    public record PatientInfo(
        int Dni,
        string FullName
    );

    public record DoctorInfo(
        Guid DoctorId,
        string Name,
        SpecialtyInfo Specialty
    );

    public record SpecialtyInfo(
        Guid SpecialtyId,
        string Name
    );
}