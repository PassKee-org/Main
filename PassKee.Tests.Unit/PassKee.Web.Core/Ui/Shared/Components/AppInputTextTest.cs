using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PassKee.Web.Core.Ui.Shared.Components;
using PassKee.Web.Core.Ui.Shared.Components.Enums;

namespace PassKee.Tests.Unit.Web.Core.Ui.Shared.Components;

public class AppInputTextTest
{
    [Fact]
    public async Task Password_HasAssociatedLabelAndAccessibleVisibilityControl()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Label)] = "Password",
            [nameof(AppInputText.Type)] = "password"
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        var inputId = System.Text.RegularExpressions.Regex.Match(markup, "<input[^>]*id=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(inputId);
        Assert.Contains($"for=\"{inputId}\"", markup);
        Assert.Contains($"aria-controls=\"{inputId}\"", markup);
        Assert.Contains("type=\"text\"", markup);
        Assert.Contains("-webkit-text-security: disc;", markup);
        Assert.Contains("data-1p-ignore=\"true\"", markup);
        Assert.Contains("aria-label=\"Show password\"", markup);
        Assert.Contains("aria-pressed=\"false\"", markup);
        Assert.DoesNotContain("tabindex=\"-1\"", markup);
    }

    [Fact]
    public async Task Password_WhenPreventPasswordManagerFalse_RendersNativePasswordType()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Label)] = "Password",
            [nameof(AppInputText.Type)] = "password",
            [nameof(AppInputText.PreventPasswordManager)] = false
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        Assert.Contains("type=\"password\"", markup);
        Assert.DoesNotContain("-webkit-text-security", markup);
    }

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

    [Fact]
    public async Task VariantPlain_RendersBorderlessWithoutPadding()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Variant)] = InputVariant.Plain,
            [nameof(AppInputText.AriaLabel)] = "Plain field",
            [nameof(AppInputText.InputClass)] = "custom-test-class"
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        Assert.Contains("custom-test-class", markup);
        Assert.Contains("border-0 p-0", markup);
        Assert.DoesNotContain("border-slate-200", markup);
    }

    [Fact]
    public async Task Multiline_RendersTextareaWithRows()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Multiline)] = true,
            [nameof(AppInputText.Rows)] = 4,
            [nameof(AppInputText.Value)] = "Some multiline text"
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        Assert.Contains("<textarea", markup);
        Assert.Contains("rows=\"4\"", markup);
        Assert.Contains("Some multiline text", markup);
        Assert.DoesNotContain("<input", markup);
    }

    [Fact]
    public async Task Password_WhenShowPasswordToggleFalse_DoesNotRenderToggleButton()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Type)] = "password",
            [nameof(AppInputText.ShowPasswordToggle)] = false
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        Assert.DoesNotContain("aria-label=\"Show password\"", markup);
        Assert.DoesNotContain("aria-label=\"Hide password\"", markup);
    }

    [Fact]
    public async Task Password_WhenExternalVisibilityTrue_ShowsTextWithoutSecurityDisc()
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AppInputText.Type)] = "password",
            [nameof(AppInputText.IsPasswordVisible)] = true
        });

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<AppInputText>(parameters);
            return component.ToHtmlString();
        });

        Assert.DoesNotContain("-webkit-text-security", markup);
    }
}
