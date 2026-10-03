using System;
using System.Threading.Tasks;
using Domain.Abstractions;
using PassKee.Business.Common.Constants;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Api.Services.Storage;

/// <summary>
/// Resolves who may access a stored file based on the entity the file belongs to.
/// </summary>
public interface IFileStorageAccessService : IDomainService
{
    Task CheckAccessAsync(AccessLevel accessLevel, Guid userId, FileStorageEntity file);
}
