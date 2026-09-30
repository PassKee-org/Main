using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Components;
using PassKee.Web.Pages.App.Modals;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Pages.App;

public partial class VaultWelcomeBlock : BaseReactiveComponent
{
    [Inject] public IState<VaultsState> VaultsState { get; set; } = null!;

    private async Task OpenCreateVaultModal()
    {
        var result = await ModalService.ShowAsync<CreateVaultModal>();
        if (result.IsSuccess && result.Data != null)
        {
            var vaultName = (string)result.Data;
            if (!string.IsNullOrWhiteSpace(vaultName))
            {
                Dispatcher.Dispatch(new CreateVaultAction(vaultName.Trim(), string.Empty));
            }
        }
    }
}