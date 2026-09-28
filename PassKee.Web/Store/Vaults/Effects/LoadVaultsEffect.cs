using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class LoadVaultsEffect : Effect<LoadVaultsAction>
{
    private readonly IVaultClientService _vaultService;

    public LoadVaultsEffect(IVaultClientService vaultService)
    {
        _vaultService = vaultService;
    }

    public override async Task HandleAsync(LoadVaultsAction action, IDispatcher dispatcher)
    {
        try
        {
            var vaults = await _vaultService.GetVaultsAsync();
            dispatcher.Dispatch(new LoadVaultsSuccessAction(vaults));

            if (vaults.Any())
            {
                var firstVaultId = vaults.First().Id;
                dispatcher.Dispatch(new SelectVaultAction(firstVaultId));
                dispatcher.Dispatch(new LoadVaultDetailsAction(firstVaultId));
            }
        }
        catch
        {
            dispatcher.Dispatch(new LoadVaultsFailureAction());
        }
    }
}
