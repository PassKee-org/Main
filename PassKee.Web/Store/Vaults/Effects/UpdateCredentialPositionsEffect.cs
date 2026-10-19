using System;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.Extensions.Logging;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Vaults;

namespace PassKee.Web.Store.Vaults.Effects;

public class UpdateCredentialPositionsEffect : Effect<UpdateCredentialPositionsAction>
{
    private readonly IVaultClientService _vaultService;
    private readonly IToastService _toastService;
    private readonly ILogger<UpdateCredentialPositionsEffect> _logger;

    public UpdateCredentialPositionsEffect(
        IVaultClientService vaultService,
        IToastService toastService,
        ILogger<UpdateCredentialPositionsEffect> logger)
    {
        _vaultService = vaultService;
        _toastService = toastService;
        _logger = logger;
    }

    public override async Task HandleAsync(UpdateCredentialPositionsAction action, IDispatcher dispatcher)
    {
        try
        {
            if (action.VaultId == Guid.Empty || action.Positions.Count == 0)
            {
                return;
            }

            await _vaultService.UpdateCredentialPositionsAsync(action.VaultId, action.Positions);
        }
        catch (Exception e)
        {
            _toastService.ShowError("Failed to update item positions");
            _logger.LogError(e, "Error updating credential positions");
        }
    }
}

