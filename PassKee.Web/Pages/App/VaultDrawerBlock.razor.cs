using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Api.Shared.Models.Vaults;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App;

public partial class VaultDrawerBlock : ComponentBase
{
    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }
}
