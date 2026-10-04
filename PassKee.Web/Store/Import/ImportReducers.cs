using Fluxor;

namespace PassKee.Web.Store.Import;

public static class ImportReducers
{
    [ReducerMethod]
    public static ImportState Start(ImportState state, StartImportAction action)
        => state.IsRunning ? state : new ImportState { SessionId = action.SessionId, IsRunning = true };

    [ReducerMethod]
    public static ImportState Progress(ImportState state, ImportProgressAction action)
        => state.SessionId == action.SessionId && state.IsRunning ? state with { Progress = action.Progress } : state;

    [ReducerMethod]
    public static ImportState Finish(ImportState state, ImportFinishedAction action)
        => state.SessionId == action.SessionId ? state with { IsRunning = false, Result = action.Result, Error = action.Error } : state;

    [ReducerMethod]
    public static ImportState Clear(ImportState state, ClearImportAction action)
        => state.SessionId == action.SessionId && !state.IsRunning ? new ImportState() : state;
}