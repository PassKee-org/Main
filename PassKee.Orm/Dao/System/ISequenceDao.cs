using Domain.Abstractions;

namespace PassKee.Orm.Dao.System;

public interface ISequenceDao: IDomainService
{
    Task<long> GetNextValue<TEntity>(TEntity entity) where TEntity: IEntity;
}
