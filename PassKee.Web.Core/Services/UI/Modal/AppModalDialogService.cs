using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Shared.Modals;

namespace PassKee.Web.Core.Services.UI.Modal;

public class AppModalDialogService : IAppModalDialogService
{
    private readonly List<AppModalInstance> _modals = new();

    public event Action? OnModalsChanged;

    public IReadOnlyList<AppModalInstance> Modals => _modals.AsReadOnly();

    public Task<AppModalResult> ShowAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        AppModalOptions? options = null,
        Action<AppModalResult>? onClose = null
    ) where TComponent : IComponent
    {
        var instance = CreateInstance(typeof(TComponent), parameters, options, onClose);
        _modals.Add(instance);
        NotifyChanged();
        return instance.Tcs.Task;
    }

    public void Show<TComponent>(
        IDictionary<string, object?>? parameters = null,
        AppModalOptions? options = null,
        Action<AppModalResult>? onClose = null
    ) where TComponent : IComponent
    {
        _ = ShowAsync<TComponent>(parameters, options, onClose);
    }

    public async Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string? confirmText = null,
        string? cancelText = null,
        AppConfirmationType type = AppConfirmationType.Alert,
        AppModalOptions? options = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { nameof(AppConfirmationModal.Message), message },
            { nameof(AppConfirmationModal.Title), title },
            { nameof(AppConfirmationModal.ConfirmText), confirmText },
            { nameof(AppConfirmationModal.CancelText), cancelText },
            { nameof(AppConfirmationModal.Type), type }
        };

        options ??= new AppModalOptions
        {
            Size = AppModalSize.Small,
            HasCloseButton = true,
            IsCloseOnBackdropClick = true
        };

        var result = await ShowAsync<AppConfirmationModal>(parameters, options);
        return result.IsSuccess;
    }

    public Task<bool> ShowConfirmationAsync(AppConfirmationOptions options)
    {
        return ShowConfirmationAsync(
            options.Message,
            options.Title,
            options.ConfirmText,
            options.CancelText,
            options.Type,
            options.ModalOptions
        );
    }

    public void Close(AppModalInstance modalInstance, AppModalResult? result = null)
    {
        if (modalInstance == null)
        {
            return;
        }

        result ??= AppModalResult.Cancel();

        if (_modals.Remove(modalInstance))
        {
            NotifyChanged();
            modalInstance.OnClosedCallback?.Invoke(result);
            modalInstance.Tcs.TrySetResult(result);
        }
    }

    public void Close(Guid modalId, AppModalResult? result = null)
    {
        var instance = _modals.FirstOrDefault(m => m.Id == modalId);
        if (instance != null)
        {
            Close(instance, result);
        }
    }

    private AppModalInstance CreateInstance(
        Type componentType,
        IDictionary<string, object?>? parameters,
        AppModalOptions? options,
        Action<AppModalResult>? onClose
    )
    {
        return new AppModalInstance(this)
        {
            ComponentType = componentType,
            Parameters = parameters,
            Options = options ?? new AppModalOptions(),
            OnClosedCallback = onClose
        };
    }

    private void NotifyChanged()
    {
        OnModalsChanged?.Invoke();
    }
}
