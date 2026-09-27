using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Services.Http;
using PassKee.Web.Store.Auth;

namespace PassKee.Web.Components;

public class BaseReactiveComponent : FluxorComponent
{
    [Parameter]
    public string? Locale { get; set; }

    [Inject]
    protected IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    protected IJSRuntime Js { get; set; } = default!;

    [Inject]
    protected ApiService ApiService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected IState<AuthState> AuthState { get; set; } = default!;

    [Inject]
    protected IToastService ToastService { get; set; } = default!;

    [Inject]
    protected IAppModalDialogService ModalService { get; set; } = default!;

    private readonly List<Action> _actionsToRunAfterRender = [];

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        foreach (var actionToRun in _actionsToRunAfterRender)
        {
            actionToRun();
        }
        _actionsToRunAfterRender.Clear();
        return base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Run an action once after the component is rendered
    /// </summary>
    /// <param name="action">Action to invoke after render</param>
    protected void RunAfterRendered(Action action) => _actionsToRunAfterRender.Add(action);
}
