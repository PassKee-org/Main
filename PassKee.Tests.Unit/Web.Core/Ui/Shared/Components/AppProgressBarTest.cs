using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PassKee.Web.Core.Ui.Shared.Components.Progress;

namespace PassKee.Tests.Unit.Web.Core.Ui.Shared.Components;

public class AppProgressBarTest
{
    [Theory]
    [InlineData(-10, "0")]
    [InlineData(25, "25")]
    [InlineData(120, "100")]
    [InlineData(double.NaN, "0")]
    public async Task ClampsValueAndProvidesAccessiblePercentage(double value, string expected)
    {
        var markup = await Render(value, false);
        Assert.Contains("role=\"progressbar\"", markup);
        Assert.Contains($"aria-valuenow=\"{expected}\"", markup);
        Assert.Contains($"{expected}%", markup);
    }

    [Fact]
    public async Task OpeningDoesNotClaimPercentage()
    {
        var markup = await Render(0, true);
        Assert.DoesNotContain("aria-valuenow", markup);
        Assert.DoesNotContain("0%", markup);
        Assert.Contains("Opening database", markup);
    }

    private static async Task<string> Render(double value, bool indeterminate)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppProgressBar>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(AppProgressBar.Value)] = value,
                [nameof(AppProgressBar.Indeterminate)] = indeterminate,
                [nameof(AppProgressBar.Label)] = "Opening database"
            }));
            return component.ToHtmlString();
        });
    }
}