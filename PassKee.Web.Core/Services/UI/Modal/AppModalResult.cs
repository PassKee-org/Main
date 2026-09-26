namespace PassKee.Web.Core.Services.UI.Modal;

public class AppModalResult
{
    public bool IsSuccess { get; }

    public bool IsCancelled { get; }

    public string? ButtonId { get; }

    public object? Data { get; }

    public AppModalResult(bool isSuccess, bool isCancelled, string? buttonId, object? data)
    {
        IsSuccess = isSuccess;
        IsCancelled = isCancelled;
        ButtonId = buttonId;
        Data = data;
    }

    public static AppModalResult Ok(string buttonId = "ok") => new(true, false, buttonId, null);

    public static AppModalResult Ok<T>(T data, string buttonId = "ok") => new(true, false, buttonId, data);

    public static AppModalResult Cancel(string buttonId = "cancel") => new(false, true, buttonId, null);

    public static AppModalResult Custom(string buttonId, object? data = null) => new(true, false, buttonId, data);
}
