using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Orm.Dao.Storage;

public class FileStorageDao : BaseDao, IFileStorageDao
{
    public FileStorageDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<FileStorageEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<FileStorageEntity>()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
