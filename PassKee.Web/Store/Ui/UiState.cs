using Fluxor;

namespace PassKee.Web.Store.Ui;

[FeatureState]
public record UiState
{
    public bool IsMainMenuOpened { get; init; } = true;
}

