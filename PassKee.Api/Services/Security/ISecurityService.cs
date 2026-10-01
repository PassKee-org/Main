using System;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Entities;

namespace PassKee.Api.Services.Security;

public interface ISecurityService : IScopedDomainService
{
    Task CheckAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity);
    Task CheckAccess<TEntity>(AccessLevel accessLevel, Guid userId, TEntity? entity);

    Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, UserEntity user, TEntity? entity);
    Task<bool> HasAccess<TEntity>(AccessLevel accessLevel, Guid userId, TEntity? entity);
}
