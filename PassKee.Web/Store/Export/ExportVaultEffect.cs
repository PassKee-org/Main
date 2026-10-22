using Fluxor;
using PassKee.Web.Services.Export;
using PassKee.Web.Store.Auth;
using PassKee.Web.Store.Vaults;

namespace PassKee.Web.Store.Export;

public sealed class ExportVaultEffect(
    IVaultExportService service,
    VaultExportSession session,
    IState<VaultsState> vaults,
    IState<AuthState> auth) : Effect<StartExportAction>
{
    public override async Task HandleAsync(StartExportAction action, IDispatcher dispatcher)
    {
        var context = session.Get(action.SessionId);
        if (context == null) return;
        var (request, token) = context.Value;
        VaultExportResult? result = null;
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
            result = await service.ExportAsync(request, progress => dispatcher.Dispatch(new ExportProgressAction(action.SessionId, progress)), token);
        }
        catch (NotSupportedException exception)
        {
            error = exception.Message;
        }
        catch (OperationCanceledException)
        {
            result = new VaultExportResult { Cancelled = true };
        }
        catch
        {
            error = "Could not create the backup file. Check that the vault is unlocked and try again.";
        }
        finally
        {
            vaults.StateChanged -= CheckSession;
            auth.StateChanged -= CheckSession;
            session.Release(action.SessionId);
            dispatcher.Dispatch(new ExportFinishedAction(action.SessionId, result, error));
        }
    }
}

