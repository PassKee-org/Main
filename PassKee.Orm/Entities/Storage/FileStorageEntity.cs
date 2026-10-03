using PassKee.Orm.Core;

namespace PassKee.Orm.Entities.Storage;

/// <summary>
/// Base record of a file stored in the cloud (Garage). Concrete subclasses bind the file to an owning entity.
/// The blob is always client-side encrypted, so the server knows neither its name nor its content.
/// </summary>
public abstract class FileStorageEntity : AEntity
{
    public virtual string CloudFilePath { get; set; } = null!;

    /// <summary>
    /// Size of the stored (encrypted) blob in bytes.
    /// </summary>
    public virtual long Size { get; set; }
}
