using PassKee.Web.Services.Export;
using PassKee.Web.Store.Export;

namespace PassKee.Tests.Unit.Web.Services.Export;

public class ExportLifecycleTests
{
    [Fact]
    public void ProgressFromOldSessionCannotOverwriteNewSession()
    {
        var state = new ExportState { SessionId = Guid.NewGuid(), IsRunning = true };
        Assert.Same(state, ExportReducers.Progress(state, new ExportProgressAction(Guid.NewGuid(), new ExportProgress(10, 10, "Done"))));
        Assert.Same(state, ExportReducers.Finish(state, new ExportFinishedAction(Guid.NewGuid(), new VaultExportResult())));
    }

    [Fact]
    public void ClearExportZeroesFileBytesAndResetsState()
    {
        var sessionId = Guid.NewGuid();
        var buffer = new byte[] { 1, 2, 3, 4 };
        var state = new ExportState
        {
            SessionId = sessionId,
            IsRunning = false,
            Result = new VaultExportResult { FileName = "backup.kdbx", FileBytes = buffer }
        };

        var cleared = ExportReducers.Clear(state, new ClearExportAction(sessionId));
        Assert.Equal(Guid.Empty, cleared.SessionId);
        Assert.Null(cleared.Result);
        Assert.All(buffer, b => Assert.Equal(0, b));
    }

    [Fact]
    public void PrivateSessionOwnsAndClearsKeyWithoutClearingVaultState()
    {
        using var session = new VaultExportSession();
        var key = Enumerable.Repeat((byte)7, 32).ToArray();
        var id = session.Begin(new VaultExportRequest(Guid.NewGuid(), "Personal", "fixture", key, [], [], []));
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

