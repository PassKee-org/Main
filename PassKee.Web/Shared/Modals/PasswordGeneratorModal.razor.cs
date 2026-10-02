using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassKee.Web.Core.Services.UI.Modal;
using PassKee.Web.Core.Services.UI.Toast;

namespace PassKee.Web.Shared.Modals;

public partial class PasswordGeneratorModal : ComponentBase
{
    [CascadingParameter] public AppModalInstance ModalInstance { get; set; } = null!;
    [Inject] public IAppModalDialogService ModalService { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    [Parameter] public int InitialLength { get; set; } = 20;

    private string _password = string.Empty;
    private string _generatorType = "Random";
    private int _length = 20;
    private bool _includeDigits = true;
    private bool _includeSymbols = true;
    private bool _includeUppercase = true;
    private bool _isCopied;

    private const string LowercaseChars = "abcdefghjkmnpqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string DigitChars = "23456789";
    private const string SymbolChars = "!@#$%&*_-+=";

    protected override void OnInitialized()
    {
        if (InitialLength >= 8 && InitialLength <= 64)
        {
            _length = InitialLength;
        }

        Regenerate();
    }

    private void Regenerate()
    {
        if (_generatorType == "Pin")
        {
            var pinChars = new char[_length];
            for (var i = 0; i < _length; i++)
            {
                pinChars[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
            }
            _password = new string(pinChars);
            return;
        }

        var pool = LowercaseChars;
        if (_includeUppercase) pool += UppercaseChars;
        if (_includeDigits) pool += DigitChars;
        if (_includeSymbols) pool += SymbolChars;

        if (pool.Length == 0) pool = LowercaseChars;

        var chars = new char[_length];

        // Ensure at least one char from each selected set
        var guaranteedCount = 0;
        chars[guaranteedCount++] = LowercaseChars[RandomNumberGenerator.GetInt32(LowercaseChars.Length)];

        if (_includeUppercase && _length > guaranteedCount)
            chars[guaranteedCount++] = UppercaseChars[RandomNumberGenerator.GetInt32(UppercaseChars.Length)];

        if (_includeDigits && _length > guaranteedCount)
            chars[guaranteedCount++] = DigitChars[RandomNumberGenerator.GetInt32(DigitChars.Length)];

        if (_includeSymbols && _length > guaranteedCount)
            chars[guaranteedCount++] = SymbolChars[RandomNumberGenerator.GetInt32(SymbolChars.Length)];

        for (var i = guaranteedCount; i < _length; i++)
        {
            chars[i] = pool[RandomNumberGenerator.GetInt32(pool.Length)];
        }

        // Shuffle
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        _password = new string(chars);
    }

    private string StrengthBarColorClass
    {
        get
        {
            if (_generatorType == "Pin")
            {
                return _length >= 8 ? "bg-emerald-500" : _length >= 6 ? "bg-blue-500" : "bg-amber-500";
            }

            if (_length < 12) return "bg-rose-500";
            if (_length < 16) return "bg-amber-500";
            if (_length < 20) return "bg-blue-500";
            return "bg-emerald-500";
        }
    }

    private string StrengthBarWidthClass
    {
        get
        {
            if (_length < 12) return "w-1/4";
            if (_length < 16) return "w-2/4";
            if (_length < 20) return "w-3/4";
            return "w-full";
        }
    }

    private void Cancel()
    {
        ModalService.Close(ModalInstance, AppModalResult.Cancel());
    }

    private void UsePassword()
    {
        ModalService.Close(ModalInstance, AppModalResult.Ok(_password));
    }

    private async Task CopyToClipboardAsync()
    {
        if (string.IsNullOrEmpty(_password)) return;

        try
        {
            await Js.InvokeVoidAsync("navigator.clipboard.writeText", _password);
            _isCopied = true;
            ToastService.ShowSuccess("Password copied to clipboard");
            StateHasChanged();
            await Task.Delay(2000);
            _isCopied = false;
            StateHasChanged();
        }
        catch
        {
            // Ignore
        }
    }
}
