using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PassKee.Web.Core.Ui.Shared.Components.Enums;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Core.Ui.Shared.Components;

public partial class TagsSelect : ComponentBase
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public ComponentSize Size { get; set; } = ComponentSize.Medium;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool FullWidth { get; set; } = true;

    [Parameter]
    public string Placeholder { get; set; } = "Select or create tags...";

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public IEnumerable<Guid>? Value { get; set; }

    [Parameter]
    public EventCallback<IEnumerable<Guid>> ValueChanged { get; set; }

    [Parameter]
    public IReadOnlyList<DecryptedTag> Tags { get; set; } = [];

    [Parameter]
    public Func<string, Task<DecryptedTag?>>? OnCreateTag { get; set; }

    protected bool _isOpen;
    protected bool _isCreating;
    protected string _searchText = string.Empty;
    protected string _newTagName = string.Empty;
    protected HashSet<Guid> _selectedIds = [];

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        var newSet = new HashSet<Guid>(Value ?? Enumerable.Empty<Guid>());
        if (!_selectedIds.SetEquals(newSet))
        {
            _selectedIds = newSet;
        }
    }

    protected IEnumerable<DecryptedTag> _selectedTags =>
        Tags.Where(t => _selectedIds.Contains(t.Id));

    protected IEnumerable<DecryptedTag> FilteredTags
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return Tags;
            }

            return Tags.Where(t => t.Name.Contains(_searchText.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    protected bool CanCreateFromSearch =>
        OnCreateTag != null
        && !string.IsNullOrWhiteSpace(_searchText)
        && !Tags.Any(t => string.Equals(t.Name, _searchText.Trim(), StringComparison.OrdinalIgnoreCase));

    protected string BorderAndRingClasses => _isOpen
        ? "border-blue-500 ring-3 ring-blue-500/15 shadow-sm"
        : "border-slate-200 dark:border-slate-700 hover:border-slate-300 dark:hover:border-slate-600";

    protected void ToggleOpen()
    {
        if (Disabled) return;
        _isOpen = !_isOpen;
        if (!_isOpen)
        {
            _searchText = string.Empty;
            _newTagName = string.Empty;
        }
    }

    protected void Close()
    {
        _isOpen = false;
        _searchText = string.Empty;
        _newTagName = string.Empty;
    }

    protected async Task ToggleTagAsync(Guid tagId)
    {
        if (Disabled) return;

        if (_selectedIds.Contains(tagId))
        {
            _selectedIds.Remove(tagId);
        }
        else
        {
            _selectedIds.Add(tagId);
        }

        await ValueChanged.InvokeAsync(_selectedIds.ToList());
    }

    protected async Task RemoveTagAsync(Guid tagId)
    {
        if (Disabled) return;

        if (_selectedIds.Remove(tagId))
        {
            await ValueChanged.InvokeAsync(_selectedIds.ToList());
        }
    }

    protected async Task ClearAsync()
    {
        if (Disabled) return;

        _selectedIds.Clear();
        await ValueChanged.InvokeAsync(_selectedIds.ToList());
    }

    protected void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
    }

    protected void OnNewTagInput(ChangeEventArgs e)
    {
        _newTagName = e.Value?.ToString() ?? string.Empty;
    }

    protected async Task OnNewTagKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await CreateNewTagAsync();
        }
    }

    protected async Task CreateTagFromSearchAsync()
    {
        if (_isCreating || string.IsNullOrWhiteSpace(_searchText) || OnCreateTag == null) return;

        var nameToCreate = _searchText.Trim();
        await CreateTagInternalAsync(nameToCreate);
    }

    protected async Task CreateNewTagAsync()
    {
        if (_isCreating || string.IsNullOrWhiteSpace(_newTagName) || OnCreateTag == null) return;

        var nameToCreate = _newTagName.Trim();
        await CreateTagInternalAsync(nameToCreate);
    }

    private async Task CreateTagInternalAsync(string name)
    {
        _isCreating = true;
        try
        {
            // If already exists, simply select it
            var existing = Tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _selectedIds.Add(existing.Id);
                await ValueChanged.InvokeAsync(_selectedIds.ToList());
                _searchText = string.Empty;
                _newTagName = string.Empty;
                return;
            }

            var created = await OnCreateTag!(name);
            if (created != null)
            {
                _selectedIds.Add(created.Id);
                await ValueChanged.InvokeAsync(_selectedIds.ToList());
                _searchText = string.Empty;
                _newTagName = string.Empty;
            }
        }
        finally
        {
            _isCreating = false;
        }
    }
}
