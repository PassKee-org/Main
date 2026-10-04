using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class TagsBadgesList : ComponentBase
{
    [Parameter]
    public IEnumerable<DecryptedTag>? Tags { get; set; }

    [Parameter]
    public IEnumerable<string>? TagNames { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    protected IEnumerable<string> ResolvedNames
    {
        get
        {
            if (TagNames != null)
            {
                return TagNames.Where(n => !string.IsNullOrWhiteSpace(n));
            }

            if (Tags != null)
            {
                return Tags.Select(t => t.Name).Where(n => !string.IsNullOrWhiteSpace(n));
            }

            return Enumerable.Empty<string>();
        }
    }
}
