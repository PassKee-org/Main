using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Services.UI.Modal;

public interface IAppModalDialogService
{
    event Action? OnModalsChanged;

    IReadOnlyList<AppModalInstance> Modals { get; }

    Task<AppModalResult> ShowAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        AppModalOptions? options = null,
        Action<AppModalResult>? onClose = null
    ) where TComponent : IComponent;

    void Show<TComponent>(
        IDictionary<string, object?>? parameters = null,
        AppModalOptions? options = null,
        Action<AppModalResult>? onClose = null
    ) where TComponent : IComponent;

    Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string? confirmText = null,
        string? cancelText = null,
        AppConfirmationType type = AppConfirmationType.Alert,
        AppModalOptions? options = null
    );

    Task<bool> ShowConfirmationAsync(AppConfirmationOptions options);

    void Close(AppModalInstance modalInstance, AppModalResult? result = null);

    void Close(Guid modalId, AppModalResult? result = null);
}
