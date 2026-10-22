using Fluxor;

namespace PassKee.Web.Store.Export;

public static class ExportReducers
{
    [ReducerMethod]
    public static ExportState Start(ExportState state, StartExportAction action)
        => state.IsRunning ? state : new ExportState { SessionId = action.SessionId, IsRunning = true };

    [ReducerMethod]
    public static ExportState Progress(ExportState state, ExportProgressAction action)
        => state.SessionId == action.SessionId && state.IsRunning ? state with { Progress = action.Progress } : state;

    [ReducerMethod]
    public static ExportState Finish(ExportState state, ExportFinishedAction action)
        => state.SessionId == action.SessionId ? state with { IsRunning = false, Result = action.Result, Error = action.Error } : state;

    [ReducerMethod]
    public static ExportState Clear(ExportState state, ClearExportAction action)
    {
        if (state.SessionId != action.SessionId || state.IsRunning)
        {
            return state;
        }

        if (state.Result?.FileBytes is { Length: > 0 })
        {
            Array.Clear(state.Result.FileBytes);
        }

        return new ExportState();
    }
}

