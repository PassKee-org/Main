using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App.Mobile.Parts;

public partial class MobileDirectoryRowBlock : ComponentBase
{
    [Parameter] public required DecryptedDirectory Directory { get; set; }
    [Parameter] public int ItemCount { get; set; }
    [Parameter] public int SubdirectoryCount { get; set; }
    [Parameter] public bool IsDropUp { get; set; }

    [Parameter] public EventCallback<Guid?> OnSelect { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnAddSubdirectory { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnEdit { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnMove { get; set; }
    [Parameter] public EventCallback<DecryptedDirectory> OnDelete { get; set; }

    private Task HandleClick() => OnSelect.InvokeAsync(Directory.Id);
}

