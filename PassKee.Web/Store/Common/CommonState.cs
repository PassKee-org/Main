using Fluxor;

namespace PassKee.Web.Store.Common;

[FeatureState]
public record CommonState
{
    public bool IsInitialized { get; init; }
    
    public bool IsAppInitialized { get; init; }
}

