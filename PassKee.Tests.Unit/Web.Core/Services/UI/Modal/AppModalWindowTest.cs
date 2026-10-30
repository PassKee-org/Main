using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Ui.Shared.Components.Modal;
using AppConfirmationModal = PassKee.Web.Core.Shared.Modals.AppConfirmationModal;

namespace PassKee.Tests.Unit.Web.Core.Services.UI.Modal;

[SuppressMessage("Usage", "BL0005", Justification = "These event-dispatch tests instantiate the component without a renderer.")]
public class AppModalWindowTest
{
    [Fact]
    public async Task HandleEventAsync_InvokesKeyboardCallbacksWithoutRequestingRender()
    {
        var service = new AppModalDialogService();
        var modalTask = service.ShowAsync<AppConfirmationModal>();
        var window = new AppModalWindow { ModalInstance = Assert.Single(service.Modals) };
        var receivedKeys = new List<string>();
        var callback = new EventCallbackWorkItem((Action<KeyboardEventArgs>)(arguments => receivedKeys.Add(arguments.Key)));

        foreach (var key in new[] { "a", "b", "c" })
        {
            await ((IHandleEvent)window).HandleEventAsync(callback, new KeyboardEventArgs { Key = key });
        }

        Assert.Equal(new[] { "a", "b", "c" }, receivedKeys);
        Assert.False(modalTask.IsCompleted);
    }

    [Fact]
    public async Task HandleEventAsync_AllowsServiceToCloseModal()
    {
        var service = new AppModalDialogService();
        var modalTask = service.ShowAsync<AppConfirmationModal>();
        var instance = Assert.Single(service.Modals);
        var window = new AppModalWindow { ModalInstance = instance };
        var callback = new EventCallbackWorkItem((Func<KeyboardEventArgs, Task>)(_ => instance.Close(AppModalResult.Cancel("escape"))));

        await ((IHandleEvent)window).HandleEventAsync(callback, new KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(service.Modals);
        Assert.False((await modalTask).IsSuccess);
    }

    [Fact]
    public async Task CloseAll_WhenMultipleModalsOpen_ClosesAllAndCompletesTasksAsCancelled()
    {
        var service = new AppModalDialogService();
        var modalsChangedFired = false;
        service.OnModalsChanged += () => modalsChangedFired = true;

        var modal1Task = service.ShowAsync<AppConfirmationModal>();
        var modal2Task = service.ShowAsync<AppConfirmationModal>();

        Assert.Equal(2, service.Modals.Count);

        service.CloseAll();

        Assert.Empty(service.Modals);
        Assert.True(modalsChangedFired);

        var result1 = await modal1Task;
        var result2 = await modal2Task;

        Assert.True(result1.IsCancelled);
        Assert.False(result1.IsSuccess);
        Assert.True(result2.IsCancelled);
        Assert.False(result2.IsSuccess);
    }

    [Fact]
    public void CloseAll_WhenNoModalsOpen_DoesNotThrowOrNotify()
    {
        var service = new AppModalDialogService();
        var modalsChangedFired = false;
        service.OnModalsChanged += () => modalsChangedFired = true;

        service.CloseAll();

        Assert.Empty(service.Modals);
        Assert.False(modalsChangedFired);
    }

    [Fact]
    public async Task CloseAll_SuppressesCallbackExceptionsAndCompletesAllTasks()
    {
        var service = new AppModalDialogService();
        var modal1Closed = false;
        var modal2Closed = false;

        var modal1Task = service.ShowAsync<AppConfirmationModal>(onClose: _ =>
        {
            modal1Closed = true;
            throw new InvalidOperationException("Simulated error in modal callback");
        });

        var modal2Task = service.ShowAsync<AppConfirmationModal>(onClose: _ =>
        {
            modal2Closed = true;
        });

        service.CloseAll();

        Assert.Empty(service.Modals);
        Assert.True(modal1Closed);
        Assert.True(modal2Closed);

        var result1 = await modal1Task;
        var result2 = await modal2Task;

        Assert.True(result1.IsCancelled);
        Assert.True(result2.IsCancelled);
    }
}
