using System;
using AutoMapper;
using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;
using PassKee.Orm.Entities;

namespace PassKee.Api.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, AuthResponse>()
            .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
            .ForMember(dest => dest.UserPublicKey, opt => opt.MapFrom(src => src.UserPublicKey != null ? Convert.ToBase64String(src.UserPublicKey) : null))
            .ForMember(dest => dest.EncryptedUserPrivateKey, opt => opt.MapFrom(src => src.EncryptedUserPrivateKey != null ? Convert.ToBase64String(src.EncryptedUserPrivateKey) : null))
            .ForMember(dest => dest.EncryptedUserVaultKey, opt => opt.MapFrom(src => src.EncryptedUserVaultKey != null ? Convert.ToBase64String(src.EncryptedUserVaultKey) : null));

        CreateMap<UserEntity, LoginParamsResponse>()
            .ForMember(dest => dest.AuthSalt, opt => opt.MapFrom(src => src.AuthSalt != null ? Convert.ToBase64String(src.AuthSalt) : string.Empty))
            .ForMember(dest => dest.KdfParams, opt => opt.MapFrom(src => src.KdfParams));
    }
}
