using Fluxor;
using PassKee.Web.Services.Import;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Store.Import;

public sealed class ImportVaultEffect(IVaultImportService service, VaultImportSession session, IState<VaultsState> vaults, IState<AuthState> auth) : Effect<StartImportAction>
{
    public override async Task HandleAsync(StartImportAction action, IDispatcher dispatcher)
    {
        var context = session.Get(action.SessionId);
        if (context == null) return;
        var (request, token) = context.Value;
        VaultImportResult? result = null;
        string? error = null;
        void CheckSession(object? sender, EventArgs arguments)
        {
            if (vaults.Value.ActiveVaultId != request.VaultId || vaults.Value.ActiveVaultKey == null || auth.Value.UserPrivateKey == null)
                session.Cancel(action.SessionId);
        }
        vaults.StateChanged += CheckSession;
        auth.StateChanged += CheckSession;
        try
        {
            CheckSession(null, EventArgs.Empty);
            token.ThrowIfCancellationRequested();
            await Task.Delay(1, token);
            result = await service.ImportAsync(request, progress => dispatcher.Dispatch(new ImportProgressAction(action.SessionId, progress)), token);
        }
        catch (NotSupportedException exception)
        {
            error = exception.Message;
        }
        catch (OperationCanceledException)
        {
            result = new VaultImportResult { Cancelled = true };
        }
        catch
        {
            error = "Could not open the database. Check the password and file integrity. Databases requiring a key file are not supported.";
        }
        finally
        {
            vaults.StateChanged -= CheckSession;
            auth.StateChanged -= CheckSession;
            session.Release(action.SessionId);
            dispatcher.Dispatch(new ImportFinishedAction(action.SessionId, result, error));
            if (result?.HasWrites == true && vaults.Value.ActiveVaultId == request.VaultId && auth.Value.UserPrivateKey != null)
                dispatcher.Dispatch(new LoadVaultDetailsAction(request.VaultId));
        }
    }
}