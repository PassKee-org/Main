using PassKee.Api.Shared.Dto.Entity.Common;

namespace PassKee.Api.Shared.Dto.Entity;

public class UserDto : BaseDto
{
    public string Email { get; set; } = string.Empty;
}
