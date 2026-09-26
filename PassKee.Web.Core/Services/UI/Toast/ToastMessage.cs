namespace PassKee.Web.Core.Services.UI.Toast;

public record ToastMessage(
    Guid Id,
    ToastType Type,
    string Message,
    DateTime CreatedAt,
    int DurationMs
);
