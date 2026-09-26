using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI;
using PassKee.Web.Core.Services.UI.Toast;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppToastContainer : ComponentBase, IDisposable
{
    [Inject]
    protected IToastService ToastService { get; set; } = default!;

    protected override void OnInitialized()
    {
        ToastService.OnToastsUpdated += HandleToastsUpdated;
    }

    private void HandleToastsUpdated()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ToastService.OnToastsUpdated -= HandleToastsUpdated;
    }
}
