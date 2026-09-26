using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Services.UI;
using PassKee.Web.Core.Services.UI.Toast;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class AppToastItem : ComponentBase
{
    [Parameter, EditorRequired]
    public ToastMessage Toast { get; set; } = default!;

    [Parameter]
    public EventCallback OnDismiss { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    protected async Task HandleDismiss()
    {
        if (OnDismiss.HasDelegate)
        {
            await OnDismiss.InvokeAsync();
        }
    }

    protected void HandleMouseEnter()
    {
    }

    protected void HandleMouseLeave()
    {
    }

    protected string ComputedClass
    {
        get
        {
            var baseClasses = "app-toast-item pointer-events-auto flex w-full items-center gap-3 rounded-2xl border px-4 py-3.5 shadow-xl backdrop-blur-md transition-all duration-200";

            var themeClasses = Toast.Type switch
            {
                ToastType.Success =>
                    "bg-white/95 border-emerald-200/90 text-slate-800 shadow-emerald-500/10 dark:bg-slate-900/95 dark:border-emerald-800/60 dark:text-slate-100",
                ToastType.Error =>
                    "bg-white/95 border-rose-200/90 text-slate-800 shadow-rose-500/10 dark:bg-slate-900/95 dark:border-rose-800/60 dark:text-slate-100",
                ToastType.Warning =>
                    "bg-white/95 border-amber-200/90 text-slate-800 shadow-amber-500/10 dark:bg-slate-900/95 dark:border-amber-800/60 dark:text-slate-100",
                ToastType.Info =>
                    "bg-white/95 border-sky-200/90 text-slate-800 shadow-sky-500/10 dark:bg-slate-900/95 dark:border-sky-800/60 dark:text-slate-100",
                _ =>
                    "bg-white/95 border-slate-200/90 text-slate-800 shadow-slate-500/10 dark:bg-slate-900/95 dark:border-slate-700/60 dark:text-slate-100"
            };

            return $"{baseClasses} {themeClasses} {Class}".Trim();
        }
    }

    protected string IconWrapperClass => Toast.Type switch
    {
        ToastType.Success => "flex h-7 w-7 shrink-0 items-center justify-center rounded-xl bg-emerald-100 text-emerald-600 dark:bg-emerald-950/80 dark:text-emerald-400",
        ToastType.Error => "flex h-7 w-7 shrink-0 items-center justify-center rounded-xl bg-rose-100 text-rose-600 dark:bg-rose-950/80 dark:text-rose-400",
        ToastType.Warning => "flex h-7 w-7 shrink-0 items-center justify-center rounded-xl bg-amber-100 text-amber-600 dark:bg-amber-950/80 dark:text-amber-400",
        ToastType.Info => "flex h-7 w-7 shrink-0 items-center justify-center rounded-xl bg-sky-100 text-sky-600 dark:bg-sky-950/80 dark:text-sky-400",
        _ => "flex h-7 w-7 shrink-0 items-center justify-center rounded-xl bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400"
    };

    protected string IconClass => Toast.Type switch
    {
        ToastType.Success => "fa-solid fa-circle-check text-sm",
        ToastType.Error => "fa-solid fa-circle-exclamation text-sm",
        ToastType.Warning => "fa-solid fa-triangle-exclamation text-sm",
        ToastType.Info => "fa-solid fa-circle-info text-sm",
        _ => "fa-solid fa-bell text-sm"
    };
}
