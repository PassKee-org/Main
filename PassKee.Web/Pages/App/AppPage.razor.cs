using System.Threading.Tasks;
using PassKee.Web.Components;
using PassKee.Web.Core.Services.UI.Modal;

namespace PassKee.Web.Pages.App;

public partial class AppPage : BaseComponent
{


    protected async Task LockVaultAsync()
    {
        var confirmed = await ModalService.ShowConfirmationAsync(
            "Are you sure you want to lock your PassKee vault? Your decrypted keys will be cleared from memory.",
            "Lock Vault",
            "Lock Now",
            "Keep Unlocked",
            AppConfirmationType.Alert);

        if (confirmed)
        {
            ToastService.ShowInfo("Vault locked successfully.");
            NavigationManager.NavigateTo("/login");
        }
    }

    protected void AddNewItemAsync()
    {
        ToastService.ShowInfo("Add Item dialog will be available in the next release.");
    }

    protected void TriggerSuccessToast()
    {
        ToastService.ShowSuccess("Vault synced securely with zero-knowledge encryption.");
    }

    protected void TriggerErrorToast()
    {
        ToastService.ShowError("Failed to decrypt entry. Master key mismatch.");
    }

    protected void TriggerWarningToast()
    {
        ToastService.ShowWarning("Password strength is weak for GitHub account.");
    }

    protected void TriggerInfoToast()
    {
        ToastService.ShowInfo("New device authorized from 192.168.1.100.");
    }

    protected async Task TriggerConfirmModal()
    {
        var confirmed = await ModalService.ShowConfirmationAsync(
            "This action demonstrates the modal dialog component ported from TimeVic. Proceed?",
            "Confirmation Dialog",
            "Proceed",
            "Dismiss",
            AppConfirmationType.Info);

        if (confirmed)
        {
            ToastService.ShowSuccess("You confirmed the modal dialog action!");
        }
        else
        {
            ToastService.ShowWarning("Modal dialog was cancelled.");
        }
    }

    protected void CopyPassword(string title)
    {
        ToastService.ShowSuccess($"Password for {title} copied to clipboard!");
    }
}

