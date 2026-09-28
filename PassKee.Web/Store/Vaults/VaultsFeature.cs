using System;
using System.Collections.Generic;
using Fluxor;

namespace PassKee.Web.Store.Vaults;

public class VaultsFeature : Feature<VaultsState>
{
    public override string GetName() => "Vaults";

    protected override VaultsState GetInitialState() => new VaultsState(
        isLoading: false,
        vaults: new(),
        activeVaultId: null,
        activeVaultKey: null,
        directories: new(),
        credentials: new()
    );
}
