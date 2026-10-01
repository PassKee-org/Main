using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Orm.Core;

namespace PassKee.Orm.Dao.Common;

public interface IBaseDao : IDomainService
{
    Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : AEntity;
    Task DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : AEntity;
}

