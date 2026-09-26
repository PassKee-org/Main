using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Dto.Entity;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class UserAvatarBlock : ComponentBase
{
    [Parameter]
    public UserDto? User { get; set; }

    [Parameter]
    public string? Src { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Initials { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string SizeClass { get; set; } = "h-9 w-9";

    private string ResolvedInitials => !string.IsNullOrWhiteSpace(Initials)
        ? Initials
        : (!string.IsNullOrWhiteSpace(Name)
            ? Name.Substring(0, Math.Min(2, Name.Length)).ToUpperInvariant()
            : (!string.IsNullOrWhiteSpace(User?.Email)
                ? User.Email.Substring(0, Math.Min(2, User.Email.Length)).ToUpperInvariant()
                : "?"));

    private string TitleText => !string.IsNullOrWhiteSpace(Name) ? Name : (User?.Email ?? string.Empty);

    private string AltText => TitleText;

    private string AvatarKey => !string.IsNullOrWhiteSpace(Src)
        ? Src
        : $"avatar-empty-{User?.Id}";

    private string? AvatarUrl => Src;

    private string AvatarClass => string.Join(
        " ",
        new[]
        {
            "inline-flex shrink-0 items-center justify-center rounded-full bg-slate-900 object-cover text-xs font-semibold uppercase text-white select-none",
            SizeClass,
            Class
        }.Where(item => !string.IsNullOrWhiteSpace(item))
    );
}
