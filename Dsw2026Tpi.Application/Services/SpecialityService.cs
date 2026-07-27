using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;

    public SpecialityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<SpecialityModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null)
    {
        ValidatePage(pageSize, pageIndex);
        ValidateOptionalName(name);

        var specialities = await _persistence.Paginate<Speciality, string>(
            pageSize,
            pageIndex,
            s => !s.Deleted &&
                 (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)),
            s => s.Name);

        return specialities.Map(MapResponse);
    }

    public async Task<SpecialityModel.Response> GetById(Guid id)
    {
        var speciality = await _persistence.First<Speciality>(
            s => s.Id == id && !s.Deleted);

        if (speciality is null)
        {
            throw new KeyNotFoundException("La especialidad no existe.");
        }

        return MapResponse(speciality);
    }

    public async Task<SpecialityModel.Response> Create(
        SpecialityModel.Request request)
    {
        ValidateRequest(request);

        var duplicated = await _persistence.First<Speciality>(
            s => !s.Deleted && s.Name == request.Name);

        if (duplicated is not null)
        {
            throw new InvalidOperationException(
                "Ya existe una especialidad con ese nombre.");
        }

        var speciality = new Speciality(
            request.Name.Trim(),
            request.Description.Trim());

        var created = await _persistence.Add(speciality);

        return MapResponse(created);
    }

    public async Task<SpecialityModel.Response> Update(
        Guid id,
        SpecialityModel.Request request)
    {
        ValidateRequest(request);

        var speciality = await _persistence.First<Speciality>(
            s => s.Id == id && !s.Deleted);

        if (speciality is null)
        {
            throw new KeyNotFoundException("La especialidad no existe.");
        }

        var duplicated = await _persistence.First<Speciality>(
            s => !s.Deleted &&
                 s.Id != id &&
                 s.Name == request.Name);

        if (duplicated is not null)
        {
            throw new InvalidOperationException(
                "Ya existe otra especialidad con ese nombre.");
        }

        speciality.Update(
            request.Name.Trim(),
            request.Description.Trim());

        var updated = await _persistence.Update(speciality);

        return MapResponse(updated);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.First<Speciality>(
            s => s.Id == id && !s.Deleted);

        if (speciality is null)
        {
            throw new KeyNotFoundException("La especialidad no existe.");
        }

        speciality.Delete();

        await _persistence.Update(speciality);
    }

    private static SpecialityModel.Response MapResponse(
        Speciality speciality)
    {
        return new SpecialityModel.Response(
            speciality.Id,
            speciality.Name,
            speciality.Description);
    }

    private static void ValidateRequest(
        SpecialityModel.Request request)
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

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException(
                "La descripción es obligatoria.");
        }

        if (request.Description.Trim().Length is < 10 or > 100)
        {
            throw new ArgumentException(
                "La descripción debe tener entre 10 y 100 caracteres.");
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