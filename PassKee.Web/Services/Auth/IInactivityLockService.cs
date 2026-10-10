using System;
using System.Threading.Tasks;

namespace PassKee.Web.Services.Auth;

public interface IInactivityLockService : IAsyncDisposable
{
    TimeSpan InactivityTimeout { get; set; }
    TimeSpan CheckInterval { get; set; }
    bool IsRunning { get; }
    void Initialize();
    Task RecordActivityAsync();
    Task CheckInactivityAsync();
    Task LockAsync();
}
