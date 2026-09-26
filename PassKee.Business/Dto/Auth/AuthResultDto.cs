using PassKee.Orm.Entities;

namespace PassKee.Business.Dto.Auth;

public record AuthResultDto(
    string JwtToken,
    UserEntity User
);
