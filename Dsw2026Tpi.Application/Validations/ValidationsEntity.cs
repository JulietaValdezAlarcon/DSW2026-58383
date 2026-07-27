using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Validations;

public class ValidationsEntity<T> where T : EntityBase, IValidationsEntity<T>
{
    private readonly IPersistence _persistence;

    public ValidationsEntity(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<T> ValidateEntityGetById(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (string.IsNullOrWhiteSpace(entity.Id.ToString())) throw new ArgumentException("Id cannot be empty");
        var persistedEntity = await _persistence.GetById<T>(entity.Id);
        if (persistedEntity == null) throw new EntityNotFoundException($"{typeof(T).Name} not found");
        return persistedEntity;
    }

    public async Task<T> ValidateEntityFiltered(T entity, Expression<Func<T, bool>> filter)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (string.IsNullOrWhiteSpace(entity.Id.ToString())) throw new ArgumentException("Id cannot be empty");
        var persistedEntity = await _persistence.First<T>(filter);
        if (persistedEntity == null) throw new EntityNotFoundException($"{typeof(T).Name} not found");
        return persistedEntity;
    }
}
