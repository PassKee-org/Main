using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Dropdown;
using PassKee.Web.Core.Utils;

namespace PassKee.Web.Core.Ui.Shared.Components.Emoji;

public partial class SelectEmojiButton : ComponentBase
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<EmojiList.EmojiOptionModel> OnSelected { get; set; }

    [Parameter]
    public EventCallback OnReset { get; set; }

    [Parameter]
    public RenderFragment? Trigger { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string PanelClass { get; set; } = string.Empty;

    [Parameter]
    public bool ShowResetButton { get; set; } = true;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string Title { get; set; } = "Select emoji icon";

    [Parameter]
    public DropdownAlignment Alignment { get; set; } = DropdownAlignment.Left;

    private bool _isOpen;
    private string _searchQuery = string.Empty;
    private string _selectedCategory = "All";
    private readonly List<string> _categories = ["All"];

    protected string AlignmentClass => Alignment switch
    {
        DropdownAlignment.Left => "left-0",
        DropdownAlignment.Right => "right-0",
        _ => "left-0"
    };

    protected override void OnInitialized()
    {
        var distinctCategories = EmojiList.List
            .Select(e => e.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        _categories.AddRange(distinctCategories);
    }

    private IEnumerable<IGrouping<string, EmojiList.EmojiOptionModel>> GroupedEmojis
    {
        get
        {
            var query = _searchQuery.Trim();
            IEnumerable<EmojiList.EmojiOptionModel> source = EmojiList.List;

            if (_selectedCategory != "All")
            {
                source = source.Where(e => string.Equals(e.Category, _selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                source = source.Where(e =>
                    e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.Keywords.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    e.HtmlCode.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            return source.GroupBy(e => e.Category);
        }
    }

    private string? CustomSymbolPreview
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                return null;
            }

            var decoded = EmojiHelper.FormatEmoji(_searchQuery);
            if (string.IsNullOrWhiteSpace(decoded))
            {
                return null;
            }

            // If it's already one of the exact symbols in the list, no need for custom preview button
            if (EmojiList.List.Any(e => e.Symbol == decoded))
            {
                return null;
            }

            return decoded;
        }
    }

    private bool IsSelected(EmojiList.EmojiOptionModel emoji)
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            return false;
        }

        var formattedValue = EmojiHelper.FormatEmoji(Value);
        return string.Equals(formattedValue, emoji.Symbol, StringComparison.Ordinal) ||
               string.Equals(Value, emoji.HtmlCode, StringComparison.OrdinalIgnoreCase);
    }

    public void TogglePicker()
    {
        if (Disabled) return;
        _isOpen = !_isOpen;
        if (!_isOpen)
        {
            _searchQuery = string.Empty;
        }
    }

    public void OpenPicker()
    {
        if (Disabled) return;
        _isOpen = true;
    }

    public void ClosePicker()
    {
        _isOpen = false;
        _searchQuery = string.Empty;
    }

    private async Task SelectEmoji(EmojiList.EmojiOptionModel emoji)
    {
        Value = emoji.Symbol;
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(emoji.Symbol);
        }

        if (OnSelected.HasDelegate)
        {
            await OnSelected.InvokeAsync(emoji);
        }

        ClosePicker();
    }

    private async Task SelectCustomSymbol(string? symbol)
    {
        var formatted = EmojiHelper.FormatEmoji(symbol);
        Value = formatted;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(formatted);
        }

        ClosePicker();
    }

    private async Task ResetEmoji()
    {
        Value = null;
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(null);
        }

        if (OnReset.HasDelegate)
        {
            await OnReset.InvokeAsync();
        }

        ClosePicker();
    }

    private void SetCategory(string category)
    {
        _selectedCategory = category;
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        _searchQuery = e.Value?.ToString() ?? string.Empty;
    }

    private void ClearSearch()
    {
        _searchQuery = string.Empty;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && _isOpen)
        {
            ClosePicker();
        }
    }
}

