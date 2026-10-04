using PassKee.Web.Services.Import;
using PassKee.Web.Store.Import;
using PassKee.Web.Store.Vaults;

namespace PassKee.Tests.Unit.Web.Services.Import;

public class ImportLifecycleTests
{
    [Fact]
    public void LateVaultRefreshCannotSwitchBackToImportDestination()
    {
        var selectedVault = Guid.NewGuid();
        var state = new VaultsState { ActiveVaultId = selectedVault, ActiveVaultKey = new byte[32] };
        var result = VaultsReducers.ReduceLoadVaultDetailsSuccessAction(state,
            new LoadVaultDetailsSuccessAction(Guid.NewGuid(), new byte[32], [], [], []));
        Assert.Same(state, result);
    }

    [Fact]
    public void ProgressFromOldSessionCannotOverwriteNewSession()
    {
        var state = new ImportState { SessionId = Guid.NewGuid(), IsRunning = true };
        Assert.Same(state, ImportReducers.Progress(state, new ImportProgressAction(Guid.NewGuid(), new ImportProgress(10, 10, "Done"))));
        Assert.Same(state, ImportReducers.Finish(state, new ImportFinishedAction(Guid.NewGuid(), new VaultImportResult())));
    }

    [Fact]
    public void PrivateSessionOwnsAndClearsKeyWithoutClearingVaultState()
    {
        using var session = new VaultImportSession();
        var key = Enumerable.Repeat((byte)7, 32).ToArray();
        var id = session.Begin(new VaultImportRequest(new MemoryStream(), "fixture", Guid.NewGuid(), null, key, []));
        var context = session.Get(id)!.Value;
        Assert.NotSame(key, context.Request.VaultKey);
        session.Cancel(id);
        Assert.True(context.Token.IsCancellationRequested);
        session.Release(id);
        Assert.Null(session.Get(id));
        Assert.All(context.Request.VaultKey, value => Assert.Equal(0, value));
        Assert.All(key, value => Assert.Equal(7, value));
    }
}