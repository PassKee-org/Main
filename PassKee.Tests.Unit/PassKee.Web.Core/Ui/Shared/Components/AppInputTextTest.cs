using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PassKee.Web.Core.Ui.Shared.Components;

namespace PassKee.Tests.Unit.Web.Core.Ui.Shared.Components;

public class AppInputTextTest
{
    [Theory]
    [InlineData(null, false, true)]
    [InlineData("", false, true)]
    [InlineData("   ", false, true)]
    [InlineData("Title is required.", true, true)]
    [InlineData("Title is required.", true, false)]
    public async Task ErrorMessage_ControlsErrorStylingAndAccessibleMessage(string? errorMessage, bool hasError, bool showErrorMessage)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Label)] = "Title",
            [nameof(AppInputText.Required)] = true,
            [nameof(AppInputText.HelpText)] = "Enter a title.",
            [nameof(AppInputText.ErrorMessage)] = errorMessage,
            [nameof(AppInputText.ShowErrorMessage)] = showErrorMessage
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        if (hasError)
        {
            Assert.Contains("border-rose-500", markup);
            Assert.DoesNotContain("bg-rose-50", markup);
        }
        else
        {
            Assert.Contains("border-slate-200", markup);
            Assert.DoesNotContain("border-rose-500", markup);
        }

        if (hasError && showErrorMessage)
        {
            Assert.Contains("Title is required.", markup);
            Assert.Contains("role=\"alert\"", markup);
            Assert.Contains("aria-describedby=\"input-error-", markup);
            Assert.DoesNotContain("Enter a title.", markup);
        }
        else
        {
            Assert.Contains("Enter a title.", markup);
            Assert.DoesNotContain("Title is required.", markup);
            Assert.DoesNotContain("role=\"alert\"", markup);
            Assert.DoesNotContain("aria-describedby", markup);
        }
    }
}