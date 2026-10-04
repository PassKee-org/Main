using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Services.Import;
using PassKee.Web.Store.Import;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Shared.Modals;

public partial class KdbxImportModal : ComponentBase, IDisposable
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] private IAppModalDialogService ModalService { get; set; } = null!;
    [Inject] private IDispatcher Dispatcher { get; set; } = null!;
    [Inject] private IState<ImportState> ImportState { get; set; } = null!;
    [Inject] private IState<VaultsState> VaultsState { get; set; } = null!;
    [Inject] private VaultImportSession Session { get; set; } = null!;

    private static readonly string[] Providers = ["KeePass"];
    private Guid? _vaultId;
    private string VaultName => VaultsState.Value.Vaults.FirstOrDefault(vault => vault.Id == _vaultId)?.Name ?? string.Empty;
    private string? _provider;
    private string? _password;
    private IBrowserFile? _file;
    private Guid? _parentId;
    private Guid _sessionId;
    private int _step = 1;
    private string? _error;
    private bool _cancelling;
    private bool _disposed;
    private bool IsRunning => ImportState.Value.SessionId == _sessionId && ImportState.Value.IsRunning;
    private VaultImportResult? Result => ImportState.Value.SessionId == _sessionId ? ImportState.Value.Result : null;
    private bool CanStart => _file != null && _password != null && _vaultId.HasValue && !ImportState.Value.IsRunning && VaultsState.Value.ActiveVaultId == _vaultId && VaultsState.Value.ActiveVaultKey != null;
    private IEnumerable<Guid?> DirectoryIds => new Guid?[] { null }.Concat(VaultsState.Value.Directories.Select(directory => (Guid?)directory.Id));

    protected override void OnInitialized()
    {
        _vaultId = VaultsState.Value.ActiveVaultId;
        ImportState.StateChanged += OnImportChanged;
    }

    private void OnImportChanged(object? sender, EventArgs arguments)
    {
        if (_disposed || ImportState.Value.SessionId != _sessionId) return;
        _error = ImportState.Value.Error;
        if (!ImportState.Value.IsRunning)
        {
            _password = null;
            if (Result != null) _file = null;
        }
        _ = InvokeAsync(StateHasChanged);
    }

    private string DirectoryPath(Guid? id)
    {
        if (id == null) return "Vault root";
        var names = new List<string>();
        var visited = new HashSet<Guid>();
        while (id.HasValue && visited.Add(id.Value))
        {
            var directory = VaultsState.Value.Directories.FirstOrDefault(item => item.Id == id.Value);
            if (directory == null) break;
            names.Insert(0, directory.Name);
            id = directory.ParentDirectoryId;
        }
        return string.Join(" / ", names);
    }

    private void SelectProvider(string? provider) => _provider = provider;
    private void SelectFile(IBrowserFile? file) { _file = file; _error = null; }
    private void SetPassword(string? password) { _password = password; _error = null; }
    private void Next() => _step = 2;
    private void Back() => _step = 1;

    private void Start()
    {
        if (!CanStart || _file == null) return;
        if (_parentId.HasValue && !VaultsState.Value.Directories.Any(directory => directory.Id == _parentId))
        {
            _error = "The destination directory is no longer available.";
            return;
        }
        Stream? stream = null;
        try
        {
            stream = _file.OpenReadStream(DgNet.Keepass.ReadLimits.MaxFileBytes);
            _sessionId = Session.Begin(new VaultImportRequest(stream, _password ?? string.Empty, _vaultId!.Value, _parentId,
                VaultsState.Value.ActiveVaultKey!, VaultsState.Value.Tags.ToArray()));
            _password = null;
            _error = null;
            _cancelling = false;
            Dispatcher.Dispatch(new StartImportAction(_sessionId));
        }
        catch
        {
            stream?.Dispose();
            _error = "Could not start the import. Check that the vault is unlocked and no other import is running.";
        }
    }

    private void CancelImport()
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
        ImportState.StateChanged -= OnImportChanged;
        Session.Cancel(_sessionId);
        Dispatcher.Dispatch(new ClearImportAction(_sessionId));
        _password = null;
        _file = null;
    }
}
