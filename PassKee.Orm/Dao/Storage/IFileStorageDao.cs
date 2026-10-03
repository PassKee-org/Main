using System;
using System.Threading;
using System.Threading.Tasks;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Orm.Dao.Storage;

public interface IFileStorageDao : IBaseDao
{
    /// <summary>
    /// Returns the file as its concrete subclass (e.g. <see cref="VaultFileStorageEntity"/>).
    /// </summary>
    Task<FileStorageEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
}
