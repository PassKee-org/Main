namespace PassKee.Web.Core.Services.UI.Toast;

public interface IToastService
{
    event Action? OnToastsUpdated;

    IReadOnlyList<ToastMessage> ActiveToasts { get; }

    void Dismiss(Guid id);

    void ShowError(string summary);

    void ShowInfo(string summary);

    void ShowSuccess(string summary);

    void ShowWarning(string summary);
}
