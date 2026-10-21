using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Moq;
using PassKee.Api.Shared.Models.Vaults.Payloads;
using PassKee.Business.Common.Constants;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Shared.Modals;
using PassKee.Web.Shared.Modals.Parts;
using PassKee.Web.Store.Vaults;
using Xunit;

namespace PassKee.Tests.Unit.Web.Components;

[SuppressMessage("Usage", "BL0005", Justification = "Unit tests for modal component logic.")]
public class EditCredentialPasswordGeneratorTests
{
    [Fact]
    public async Task PasswordGeneratorModal_UsePassword_ReturnsGeneratedPasswordInResultData()
    {
        var modalService = new AppModalDialogService();
        var modalTask = modalService.ShowAsync<PasswordGeneratorModal>();
        var modalInstance = Assert.Single(modalService.Modals);

        var generatorModal = new PasswordGeneratorModal
        {
            ModalInstance = modalInstance,
            ModalService = modalService,
            InitialLength = 24
        };

        var onInitialized = typeof(PasswordGeneratorModal).GetMethod("OnInitialized", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var usePassword = typeof(PasswordGeneratorModal).GetMethod("UsePassword", BindingFlags.NonPublic | BindingFlags.Instance)!;

        onInitialized.Invoke(generatorModal, null);
        usePassword.Invoke(generatorModal, null);

        var result = await modalTask;
        Assert.True(result.IsSuccess);
        var generatedPassword = Assert.IsType<string>(result.Data);
        Assert.Equal(24, generatedPassword.Length);
        Assert.Equal("ok", result.ButtonId);
    }

    [Fact]
    public async Task PasswordGeneratorModal_StandaloneCancel_ClosesWithCancelResult()
    {
        var modalService = new AppModalDialogService();
        var modalTask = modalService.ShowAsync<PasswordGeneratorModal>();
        var modalInstance = Assert.Single(modalService.Modals);

        var generatorModal = new PasswordGeneratorModal
        {
            ModalInstance = modalInstance,
            ModalService = modalService,
            IsStandalone = true
        };

        var onInitialized = typeof(PasswordGeneratorModal).GetMethod("OnInitialized", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var cancel = typeof(PasswordGeneratorModal).GetMethod("Cancel", BindingFlags.NonPublic | BindingFlags.Instance)!;

        onInitialized.Invoke(generatorModal, null);
        cancel.Invoke(generatorModal, null);

        var result = await modalTask;
        Assert.False(result.IsSuccess);
        Assert.Equal("cancel", result.ButtonId);
    }

    [Fact]
    public async Task EditCredentialModal_SetParametersAsyncAfterInit_PreservesEditedTitleAndPassword()
    {
        var vaultStateMock = new Mock<IState<VaultsState>>();
        vaultStateMock.Setup(s => s.Value).Returns(new VaultsState { ActiveVaultId = Guid.NewGuid() });

        var modal = new EditCredentialModal();
        typeof(EditCredentialModal)
            .GetProperty("VaultsState", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(modal, vaultStateMock.Object);

        var onInitialized = typeof(EditCredentialModal).GetMethod("OnInitialized", BindingFlags.NonPublic | BindingFlags.Instance)!;
        onInitialized.Invoke(modal, null);

        modal.Title = "Updated Title";
        modal.Password = "generated-new-password";

        // Simulate parent DynamicComponent re-supplying initial ModalInstance.Parameters
        var initialParams = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { nameof(EditCredentialModal.Title), string.Empty },
            { nameof(EditCredentialModal.Password), "old-password" },
            { nameof(EditCredentialModal.Type), CredentialType.Login }
        });

        await modal.SetParametersAsync(initialParams);

        Assert.Equal("Updated Title", modal.Title);
        Assert.Equal("generated-new-password", modal.Password);
    }

    [Fact]
    public async Task CredentialEditableFieldBlock_OpenPasswordGeneratorAsync_SetsAdditionalFieldValue()
    {
        var modalServiceMock = new Mock<IAppModalDialogService>();
        modalServiceMock
            .Setup(s => s.ShowAsync<PasswordGeneratorModal>(
                It.IsAny<IDictionary<string, object?>?>(),
                It.IsAny<AppModalOptions?>(),
                It.IsAny<Action<AppModalResult>?>()))
            .ReturnsAsync(AppModalResult.Ok<string>("generated-additional-pass"));

        var field = new CredentialField
        {
            Type = CredentialFieldType.Password,
            Label = "DB Password",
            Value = "initial-secret"
        };

        var fieldChangedInvoked = false;
        var block = new CredentialEditableFieldBlock
        {
            Field = field,
            OnFieldChanged = EventCallback.Factory.Create(this, () => fieldChangedInvoked = true)
        };

        typeof(CredentialEditableFieldBlock)
            .GetProperty("ModalService", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(block, modalServiceMock.Object);

        var openGenerator = typeof(CredentialEditableFieldBlock).GetMethod("OpenPasswordGeneratorAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        // InvokeAsync(StateHasChanged) requires a render handle only after Field.Value and OnFieldChanged have completed
        try
        {
            await (Task)openGenerator.Invoke(block, null)!;
        }
        catch (InvalidOperationException)
        {
            // Expected when StateHasChanged is called on an unattached component after setting Field.Value
        }

        Assert.Equal("DB Password", field.Label);
        Assert.Equal("generated-additional-pass", field.Value);
        Assert.True(fieldChangedInvoked);
    }
}

