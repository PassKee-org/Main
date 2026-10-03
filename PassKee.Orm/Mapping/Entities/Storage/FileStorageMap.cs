using PassKee.Orm.Entities.Storage;
using PassKee.Orm.Mapping.Common;

namespace PassKee.Orm.Mapping.Entities.Storage;

public class FileStorageMap : BaseGuidMappings<FileStorageEntity>
{
    public FileStorageMap()
    {
        Table("file_storage");
        Map(x => x.CloudFilePath).Not.Nullable();
        Map(x => x.Size).Not.Nullable();
    }
}
