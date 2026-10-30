using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Moq;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;
using PassKee.Web.Shared.Modals;
using Xunit;

namespace PassKee.Tests.Unit.Web.Components;

[SuppressMessage("Usage", "BL0005", Justification = "Unit tests for modal component logic.")]
public class GuidGeneratorModalTests
{
    private static (GuidGeneratorModal Modal, Task<AppModalResult> Task, AppModalDialogService Service) CreateModal()
    {
        var modalService = new AppModalDialogService();
        var modalTask = modalService.ShowAsync<GuidGeneratorModal>();
        var modalInstance = Assert.Single(modalService.Modals);

        var modal = new GuidGeneratorModal
        {
            ModalInstance = modalInstance,
            ModalService = modalService
        };

        var jsMock = new Mock<IJSRuntime>();
        var toastMock = new Mock<IToastService>();

        typeof(GuidGeneratorModal)
            .GetProperty("Js", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(modal, jsMock.Object);

        typeof(GuidGeneratorModal)
            .GetProperty("ToastService", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(modal, toastMock.Object);

        return (modal, modalTask, modalService);
    }

    private static void Init(GuidGeneratorModal modal)
    {
        var onInitialized = typeof(GuidGeneratorModal).GetMethod("OnInitialized", BindingFlags.NonPublic | BindingFlags.Instance)!;
        onInitialized.Invoke(modal, null);
    }

    private static string GetPrimaryValue(GuidGeneratorModal modal)
    {
        var prop = typeof(GuidGeneratorModal).GetProperty("CurrentPrimaryValue", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (string)prop.GetValue(modal)!;
    }

    private static void SetField(GuidGeneratorModal modal, string fieldName, object value)
    {
        var field = typeof(GuidGeneratorModal).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(modal, value);
    }

    private static void Regenerate(GuidGeneratorModal modal)
    {
        var method = typeof(GuidGeneratorModal).GetMethod("Regenerate", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(modal, null);
    }

    [Fact]
    public void GuidGeneratorModal_OnInit_GeneratesValidUuidV7ByDefault()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        var value = GetPrimaryValue(modal);
        Assert.NotEmpty(value);
        Assert.True(Guid.TryParse(value, out var guid));
        Assert.Equal(7, guid.Version);
        Assert.Equal(36, value.Length);
        Assert.Equal(value.ToLowerInvariant(), value);
    }

    [Fact]
    public void GuidGeneratorModal_UuidV4_GeneratesValidVersion4()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        SetField(modal, "_guidVersion", "v4");
        Regenerate(modal);

        var value = GetPrimaryValue(modal);
        Assert.True(Guid.TryParse(value, out var guid));
        Assert.Equal(4, guid.Version);
    }

    [Fact]
    public void GuidGeneratorModal_GuidFormats_FormatsCorrectly()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        // No hyphens ("N")
        SetField(modal, "_guidFormat", "N");
        Regenerate(modal);
        var valueN = GetPrimaryValue(modal);
        Assert.Equal(32, valueN.Length);
        Assert.DoesNotContain("-", valueN);

        // Braces ("B")
        SetField(modal, "_guidFormat", "B");
        Regenerate(modal);
        var valueB = GetPrimaryValue(modal);
        Assert.StartsWith("{", valueB);
        Assert.EndsWith("}", valueB);

        // Parentheses ("P")
        SetField(modal, "_guidFormat", "P");
        Regenerate(modal);
        var valueP = GetPrimaryValue(modal);
        Assert.StartsWith("(", valueP);
        Assert.EndsWith(")", valueP);
    }

    [Fact]
    public void GuidGeneratorModal_Uppercase_ConvertsToUppercase()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        SetField(modal, "_isUppercase", true);
        Regenerate(modal);

        var value = GetPrimaryValue(modal);
        Assert.Equal(value.ToUpperInvariant(), value);
    }

    [Fact]
    public void GuidGeneratorModal_HexGenerator_GeneratesCorrectLength()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        SetField(modal, "_generatorType", "Hex");
        SetField(modal, "_byteLength", 32);
        Regenerate(modal);

        var value = GetPrimaryValue(modal);
        Assert.Equal(64, value.Length); // 32 bytes * 2 hex chars
    }

    [Fact]
    public void GuidGeneratorModal_Base64Generator_GeneratesValidBase64()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        SetField(modal, "_generatorType", "Base64");
        SetField(modal, "_byteLength", 32);
        SetField(modal, "_isUrlSafe", true);
        Regenerate(modal);

        var value = GetPrimaryValue(modal);
        Assert.NotEmpty(value);
        Assert.DoesNotContain("+", value);
        Assert.DoesNotContain("/", value);
        Assert.DoesNotContain("=", value);
    }

    [Fact]
    public void GuidGeneratorModal_NanoIdGenerator_GeneratesExpectedLength()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        SetField(modal, "_generatorType", "NanoId");
        SetField(modal, "_nanoIdLength", 25);
        Regenerate(modal);

        var value = GetPrimaryValue(modal);
        Assert.Equal(25, value.Length);
    }

    [Fact]
    public void GuidGeneratorModal_Quantity_GeneratesMultipleValues()
    {
        var (modal, _, _) = CreateModal();
        Init(modal);

        var setCount = typeof(GuidGeneratorModal).GetMethod("SetCount", BindingFlags.NonPublic | BindingFlags.Instance)!;
        setCount.Invoke(modal, new object[] { 5 });

        var prop = typeof(GuidGeneratorModal).GetProperty("AllValuesJoined", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var joined = (string)prop.GetValue(modal)!;
        var lines = joined.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(5, lines.Length);
    }

    [Fact]
    public async Task GuidGeneratorModal_Cancel_ClosesModalWithCancelResult()
    {
        var (modal, task, _) = CreateModal();
        Init(modal);

        var cancel = typeof(GuidGeneratorModal).GetMethod("Cancel", BindingFlags.NonPublic | BindingFlags.Instance)!;
        cancel.Invoke(modal, null);

        var result = await task;
        Assert.False(result.IsSuccess);
        Assert.Equal("cancel", result.ButtonId);
    }
}

