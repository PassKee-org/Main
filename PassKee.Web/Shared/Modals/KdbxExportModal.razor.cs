using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Services.Export;
using PassKee.Web.Store.Export;
using PassKee.Web.Store.Import;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals;

public partial class KdbxExportModal : ComponentBase, IDisposable
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] private IAppModalDialogService ModalService { get; set; } = null!;
    [Inject] private IDispatcher Dispatcher { get; set; } = null!;
    [Inject] private IState<ExportState> ExportState { get; set; } = null!;
    [Inject] private IState<ImportState> ImportState { get; set; } = null!;
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] private VaultExportSession Session { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private Guid? _vaultId;
    private string VaultName => VaultsState.Value.Vaults.FirstOrDefault(vault => vault.Id == _vaultId)?.Name ?? string.Empty;
    private string? _password;
    private string? _confirmPassword;
    private Guid _sessionId;
    private string? _error;
    private bool _cancelling;
    private bool _autoDownloaded;
    private bool _disposed;

    private bool IsRunning => ExportState.Value.SessionId == _sessionId && ExportState.Value.IsRunning;
    private VaultExportResult? Result => ExportState.Value.SessionId == _sessionId ? ExportState.Value.Result : null;

    private string? PasswordMismatchError =>
        !string.IsNullOrEmpty(_confirmPassword) && !string.Equals(_password, _confirmPassword, StringComparison.Ordinal)
            ? "Passwords do not match."
            : null;

    private bool CanStart =>
        !string.IsNullOrWhiteSpace(_password) &&
        string.Equals(_password, _confirmPassword, StringComparison.Ordinal) &&
        _vaultId.HasValue &&
        !ExportState.Value.IsRunning &&
        !ImportState.Value.IsRunning &&
        VaultsState.Value.ActiveVaultId == _vaultId &&
        VaultsState.Value.ActiveVaultKey != null;

    protected override void OnInitialized()
    {
        _vaultId = VaultsState.Value.ActiveVaultId;
        ExportState.StateChanged += OnExportChanged;
    }

    private void OnExportChanged(object? sender, EventArgs arguments)
    {
        if (_disposed || ExportState.Value.SessionId != _sessionId) return;
        _error = ExportState.Value.Error;
        if (!ExportState.Value.IsRunning)
        {
            _password = null;
            _confirmPassword = null;
        }

        _ = InvokeAsync(async () =>
        {
            StateHasChanged();
            if (!_autoDownloaded && Result is { Cancelled: false, FileBytes.Length: > 0 })
            {
                _autoDownloaded = true;
                await DownloadAsync();
            }
        });
    }

    private void SetPassword(string? password)
    {
        _password = password;
        _error = null;
    }

    private void SetConfirmPassword(string? confirmPassword)
    {
        _confirmPassword = confirmPassword;
        _error = null;
    }

    private void Start()
    {
        if (!CanStart) return;

        try
        {
            var state = VaultsState.Value;
            _sessionId = Session.Begin(new VaultExportRequest(
                _vaultId!.Value,
                VaultName,
                _password!,
                state.ActiveVaultKey!,
                state.Directories.ToArray(),
                state.Credentials.ToArray(),
                state.Tags.ToArray(),
                state.ArchivedCredentials.ToArray()));
            _password = null;
            _confirmPassword = null;
            _error = null;
            _cancelling = false;
            _autoDownloaded = false;
            Dispatcher.Dispatch(new StartExportAction(_sessionId));
        }
        catch
        {
            _error = "Could not start the backup. Check that the vault is unlocked and no other operation is running.";
        }
    }

    private async Task DownloadAsync()
    {
        if (Result is not { Cancelled: false, FileBytes.Length: > 0 } result) return;

        try
        {
            var fileName = string.IsNullOrWhiteSpace(result.FileName) ? "vault-backup.kdbx" : result.FileName;
            using var streamRef = new DotNetStreamReference(stream: new MemoryStream(result.FileBytes, writable: false));
            await JSRuntime.InvokeVoidAsync("PassKeeInterop.downloadFileFromStream", fileName, streamRef);
        }
        catch
        {
            _error = "Could not download the backup file. Try clicking Download .kdbx again.";
            StateHasChanged();
        }
    }

    private void CancelExport()
    {
        _cancelling = true;
        Session.Cancel(_sessionId);
    }

    private void Close()
    {
        if (IsRunning) return;
        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    public void Dispose()
    {
        _disposed = true;
        ExportState.StateChanged -= OnExportChanged;
        Session.Cancel(_sessionId);
        Dispatcher.Dispatch(new ClearExportAction(_sessionId));
        _password = null;
        _confirmPassword = null;
    }
}

