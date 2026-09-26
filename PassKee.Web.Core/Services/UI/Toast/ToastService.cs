namespace PassKee.Web.Core.Services.UI.Toast;

public class ToastService : IToastService
{
    private const int DefaultDurationMs = 4000;
    private const int MaxActiveToasts = 5;

    private readonly List<ToastMessage> _activeToasts = new();
    private readonly object _lock = new();

    public event Action? OnToastsUpdated;

    public IReadOnlyList<ToastMessage> ActiveToasts
    {
        get
        {
            lock (_lock)
            {
                return _activeToasts.ToList();
            }
        }
    }

    public void ShowSuccess(string summary) => AddToast(ToastType.Success, summary);

    public void ShowError(string summary) => AddToast(ToastType.Error, summary);

    public void ShowInfo(string summary) => AddToast(ToastType.Info, summary);

    public void ShowWarning(string summary) => AddToast(ToastType.Warning, summary);

    public void Dismiss(Guid id)
    {
        var isRemoved = false;
        lock (_lock)
        {
            var item = _activeToasts.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                _activeToasts.Remove(item);
                isRemoved = true;
            }
        }

        if (isRemoved)
        {
            NotifyUpdated();
        }
    }

    private void AddToast(ToastType type, string message, int durationMs = DefaultDurationMs)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var toast = new ToastMessage(Guid.NewGuid(), type, message.Trim(), DateTime.UtcNow, durationMs);
        lock (_lock)
        {
            if (_activeToasts.Count >= MaxActiveToasts)
            {
                _activeToasts.RemoveAt(0);
            }
            _activeToasts.Add(toast);
        }

        NotifyUpdated();

        if (durationMs > 0)
        {
            _ = AutoDismissAsync(toast.Id, durationMs);
        }
    }

    private async Task AutoDismissAsync(Guid id, int delayMs)
    {
        await Task.Delay(delayMs);
        Dismiss(id);
    }

    private void NotifyUpdated()
    {
        OnToastsUpdated?.Invoke();
    }
}

