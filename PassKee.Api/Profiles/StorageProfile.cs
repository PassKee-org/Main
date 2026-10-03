using AutoMapper;
using PassKee.Api.Shared.Models.Storage;
using PassKee.Orm.Entities.Storage;

namespace PassKee.Api.Profiles;

public class StorageProfile : Profile
{
    public StorageProfile()
    {
        CreateMap<FileStorageEntity, StoredFileDto>()
            .Include<VaultFileStorageEntity, StoredFileDto>();

        CreateMap<VaultFileStorageEntity, StoredFileDto>()
            .ForMember(d => d.VaultId, opt => opt.MapFrom(s => s.VaultId));
    }
}
