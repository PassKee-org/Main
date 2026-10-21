using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;

namespace PassKee.Web.Shared.Modals;

public partial class GuidGeneratorModal : ComponentBase
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] public IAppModalDialogService ModalService { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private string _generatorType = "Guid";
    private string _guidVersion = "v7";
    private string _guidFormat = "D";
    private bool _isUppercase;
    private int _byteLength = 32;
    private bool _isUrlSafe = true;
    private int _nanoIdLength = 21;
    private bool _includeNanoIdSymbols = true;
    private int _count = 1;
    private bool _isCopied;
    private string? _copiedValue;

    private readonly List<string> _generatedValues = [];

    private const string NanoIdUrlSafeAlphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
    private const string NanoIdAlphanumericAlphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private string CurrentPrimaryValue => _generatedValues.FirstOrDefault() ?? string.Empty;
    private string AllValuesJoined => string.Join(Environment.NewLine, _generatedValues);

    protected override void OnInitialized()
    {
        Regenerate();
    }

    private void SetCount(int count)
    {
        _count = Math.Clamp(count, 1, 50);
        Regenerate();
    }

    private void Regenerate()
    {
        _generatedValues.Clear();
        for (var i = 0; i < _count; i++)
        {
            _generatedValues.Add(GenerateSingle());
        }
    }

    private string GenerateSingle()
    {
        return _generatorType switch
        {
            "Guid" => GenerateGuid(),
            "Hex" => GenerateHex(),
            "Base64" => GenerateBase64(),
            "NanoId" => GenerateNanoId(),
            _ => GenerateGuid()
        };
    }

    private string GenerateGuid()
    {
        var guid = _guidVersion == "v7" ? Guid.CreateVersion7() : Guid.NewGuid();
        var formatted = guid.ToString(_guidFormat);
        return _isUppercase ? formatted.ToUpperInvariant() : formatted.ToLowerInvariant();
    }

    private string GenerateHex()
    {
        var bytes = RandomNumberGenerator.GetBytes(Math.Clamp(_byteLength, 8, 128));
        var hex = Convert.ToHexString(bytes);
        return _isUppercase ? hex : hex.ToLowerInvariant();
    }

    private string GenerateBase64()
    {
        var bytes = RandomNumberGenerator.GetBytes(Math.Clamp(_byteLength, 8, 128));
        var base64 = Convert.ToBase64String(bytes);
        if (_isUrlSafe)
        {
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
        return base64;
    }

    private string GenerateNanoId()
    {
        var alphabet = _includeNanoIdSymbols ? NanoIdUrlSafeAlphabet : NanoIdAlphanumericAlphabet;
        var length = Math.Clamp(_nanoIdLength, 4, 128);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        }

        return new string(chars);
    }

    private void Cancel()
    {
        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    private async Task CopyAndCloseAsync()
    {
        var textToCopy = AllValuesJoined;
        if (!string.IsNullOrEmpty(textToCopy))
        {
            await CopyToClipboardAsync(textToCopy);
        }
        ModalService.Close(ModalInstance, AppModalResult.Ok<string>(textToCopy));
    }

    private async Task CopyToClipboardAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", text);
            _copiedValue = text;
            _isCopied = true;
            ToastService.ShowSuccess("Copied to clipboard");
            StateHasChanged();
            await Task.Delay(2000);
            if (_copiedValue == text)
            {
                _copiedValue = null;
            }
            _isCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Ignore if clipboard access failed in browser context
        }
    }
}

