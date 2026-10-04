using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Linq;
using PassKee.Orm.Dao.Common;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Orm.Dao.Vaults;

public class TagDao : BaseDao, ITagDao
{
    public TagDao(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task<TagEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await Session.Query<TagEntity>()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IList<TagEntity>> GetByVaultId(Guid vaultId, CancellationToken cancellationToken = default)
    {
        return await Session.Query<TagEntity>()
            .Where(x => x.VaultId == vaultId && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<TagEntity> CreateAsync(Guid vaultId, byte[] encryptedName, CancellationToken cancellationToken = default)
    {
        var tag = new TagEntity
        {
            VaultId = vaultId,
            EncryptedName = encryptedName
        };
        await Session.SaveAsync(tag, cancellationToken);
        return tag;
    }

    public async Task<TagEntity> UpdateAsync(TagEntity tag, byte[] encryptedName, CancellationToken cancellationToken = default)
    {
        tag.EncryptedName = encryptedName;
        tag.UpdatedAt = DateTime.UtcNow;

        await Session.UpdateAsync(tag, cancellationToken);
        return tag;
    }
}
