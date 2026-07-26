using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IValidationsEntity <T> where T : EntityBase
{
    Task<T> ValidateEntityGetById(T entity);

    Task<T> ValidateEntityFiltered(T entity, Expression<Func<T, bool>> filter);


}
