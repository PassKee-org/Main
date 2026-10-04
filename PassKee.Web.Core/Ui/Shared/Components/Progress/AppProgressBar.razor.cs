using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components.Progress;

public partial class AppProgressBar : ComponentBase
{
    [Parameter] public double Value { get; set; }
    [Parameter] public bool Indeterminate { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    private string PercentText => (double.IsFinite(Value) ? Math.Clamp(Value, 0, 100) : 0).ToString("0", CultureInfo.InvariantCulture);
}