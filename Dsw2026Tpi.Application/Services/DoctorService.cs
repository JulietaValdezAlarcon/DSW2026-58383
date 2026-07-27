using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null)
    {
        ValidatePage(pageSize, pageIndex);
        ValidateOptionalName(name);

        var doctors = await _persistence.Paginate<Doctor, string>(
            pageSize,
            pageIndex,
            d => d.IsActive &&
                 (string.IsNullOrWhiteSpace(name) ||
                  d.Name.Contains(name)),
            d => d.Name,
            nameof(Doctor.Speciality));

        return doctors.Map(MapResponse);
    }

    public async Task<DoctorModel.Response> GetById(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(
            d => d.Id == id && d.IsActive,
            nameof(Doctor.Speciality));

        if (doctor is null)
        {
            throw new KeyNotFoundException(
                "El médico no existe.");
        }

        return MapResponse(doctor);
    }

    public async Task<DoctorModel.Response> Create(
        DoctorModel.Request request)
    {
        ValidateRequest(request);

        var speciality = await _persistence.First<Speciality>(
            s => s.Id == request.SpecialityId && !s.Deleted);

        if (speciality is null)
        {
            throw new KeyNotFoundException(
                "La especialidad indicada no existe.");
        }

        var duplicatedLicense = await _persistence.First<Doctor>(
            d => d.IsActive &&
                 d.LicenseNumber == request.LicenseNumber.Trim());

        if (duplicatedLicense is not null)
        {
            throw new InvalidOperationException(
                "Ya existe un médico con esa matrícula.");
        }

        var doctor = new Doctor(
            request.Name.Trim(),
            request.LicenseNumber.Trim(),
            speciality);

        var created = await _persistence.Add(doctor);

        return MapResponse(created);
    }

    public async Task<DoctorModel.Response> Update(
        Guid id,
        DoctorModel.Request request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.First<Doctor>(
            d => d.Id == id && d.IsActive,
            nameof(Doctor.Speciality));

        if (doctor is null)
        {
            throw new KeyNotFoundException(
                "El médico no existe.");
        }

        var speciality = await _persistence.First<Speciality>(
            s => s.Id == request.SpecialityId && !s.Deleted);

        if (speciality is null)
        {
            throw new KeyNotFoundException(
                "La especialidad indicada no existe.");
        }

        var duplicatedLicense = await _persistence.First<Doctor>(
            d => d.IsActive &&
                 d.Id != id &&
                 d.LicenseNumber == request.LicenseNumber.Trim());

        if (duplicatedLicense is not null)
        {
            throw new InvalidOperationException(
                "Ya existe otro médico con esa matrícula.");
        }

        doctor.Update(
            request.Name.Trim(),
            request.LicenseNumber.Trim(),
            speciality);

        var updated = await _persistence.Update(doctor);

        return MapResponse(updated);
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.First<Doctor>(
            d => d.Id == id && d.IsActive);

        if (doctor is null)
        {
            throw new KeyNotFoundException(
                "El médico no existe.");
        }

        doctor.Deactivate();

        await _persistence.Update(doctor);
    }

    private static DoctorModel.Response MapResponse(Doctor doctor)
    {
        return new DoctorModel.Response(
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            doctor.Speciality is null
                ? null
                : new DoctorModel.SpecialityDto(
                    doctor.Speciality.Id,
                    doctor.Speciality.Name));
    }

    private static void ValidateRequest(
        DoctorModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "El nombre es obligatorio.");
        }

        if (request.Name.Trim().Length is < 3 or > 100)
        {
            throw new ArgumentException(
                "El nombre debe tener entre 3 y 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            throw new ArgumentException(
                "La matrícula es obligatoria.");
        }

        if (request.SpecialityId == Guid.Empty)
        {
            throw new ArgumentException(
                "La especialidad es obligatoria.");
        }
    }

    private static void ValidateOptionalName(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name) &&
            name.Trim().Length is < 3 or > 100)
        {
            throw new ArgumentException(
                "El nombre debe tener entre 3 y 100 caracteres.");
        }
    }

    private static void ValidatePage(
        int pageSize,
        int pageIndex)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentException(
                "pageSize debe ser mayor que cero.");
        }

        if (pageIndex < 0)
        {
            throw new ArgumentException(
                "pageIndex no puede ser negativo.");
        }
    }
}