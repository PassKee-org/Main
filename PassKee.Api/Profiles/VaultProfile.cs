using AutoMapper;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Orm.Entities.Vaults;

namespace PassKee.Api.Profiles;

public class VaultProfile : Profile
{
    public VaultProfile()
    {
        CreateMap<VaultEntity, VaultDto>();
        CreateMap<DirectoryEntity, DirectoryDto>();
        CreateMap<CredentialEntity, CredentialDto>();
        CreateMap<TagEntity, TagDto>();
    }
}
