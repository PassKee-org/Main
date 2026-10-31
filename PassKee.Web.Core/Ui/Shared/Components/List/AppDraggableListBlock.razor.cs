using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace PassKee.Web.Core.Ui.Shared.Components.List;

public partial class AppDraggableListBlock<TItem> : ComponentBase
{
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = [];

    [Parameter, EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; } = default!;

    [Parameter]
    public RenderFragment? Placeholder { get; set; }

    [Parameter]
    public Func<TItem, object>? ItemKey { get; set; }

    [Parameter]
    public EventCallback<IList<TItem>> OnItemsReordered { get; set; }

    [Parameter]
    public bool IsDragEnabled { get; set; } = true;

    [Parameter]
    public bool UseVirtualization { get; set; } = true;

    [Parameter]
    public float ItemSize { get; set; } = 72f;

    [Parameter]
    public int OverscanCount { get; set; } = 12;

    [Parameter]
    public string Class { get; set; } = "divide-y divide-slate-100";

    [Parameter]
    public string ItemClass { get; set; } = string.Empty;

    [Parameter]
    public string DraggingItemClass { get; set; } = "opacity-60 outline outline-2 outline-sky-400 outline-offset-[-2px] bg-sky-50/40";

    [Parameter]
    public string DropTargetItemClass { get; set; } = "!border-t-2 !border-sky-500";

    private readonly List<TItem> _workingItems = [];
    private TItem? _draggingItem;
    private object? _draggingKey;
    private object? _dragOverKey;
    private bool _isDragging;
    private bool _dragRenderPending;
    private bool _isVirtualizeRefreshRequired;
    private Virtualize<TItem>? _virtualize;

    // ondragover fires hundreds of times per second — block re-renders unless something actually changed
    protected override bool ShouldRender()
    {
        if (_isDragging && !_dragRenderPending)
        {
            return false;
        }

        _dragRenderPending = false;
        return true;
    }

    protected override void OnParametersSet()
    {
        if (_isDragging)
        {
            return;
        }

        if (SyncWorkingItems())
        {
            _isVirtualizeRefreshRequired = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!_isVirtualizeRefreshRequired || _virtualize == null)
        {
            return;
        }

        _isVirtualizeRefreshRequired = false;
        await _virtualize.RefreshDataAsync();
    }

    private void OnDragStart(TItem item)
    {
        if (!IsDragEnabled)
        {
            return;
        }

        _isDragging = true;
        _draggingItem = item;
        _draggingKey = GetKey(item);
        _dragRenderPending = true;
    }

    private void OnDragOver(TItem item)
    {
        if (!IsDragEnabled || _draggingKey == null)
        {
            return;
        }

        var itemKey = GetKey(item);
        var targetKey = Equals(_draggingKey, itemKey) ? null : itemKey;
        if (Equals(_dragOverKey, targetKey))
        {
            return;
        }

        _dragOverKey = targetKey;
        _dragRenderPending = true;
    }

    private async Task OnDropAsync(TItem targetItem)
    {
        _isDragging = false;
        _dragRenderPending = true;

        var targetKey = GetKey(targetItem);
        if (!IsDragEnabled || _draggingKey == null || Equals(_draggingKey, targetKey))
        {
            ResetDragState();
            return;
        }

        var fromIndex = _workingItems.FindIndex(x => Equals(GetKey(x), _draggingKey));
        var toIndex = _workingItems.FindIndex(x => Equals(GetKey(x), targetKey));

        if (fromIndex < 0 || toIndex < 0)
        {
            ResetDragState();
            return;
        }

        var movedItem = _workingItems[fromIndex];
        _workingItems.RemoveAt(fromIndex);
        _workingItems.Insert(toIndex, movedItem);

        ResetDragState();
        _isVirtualizeRefreshRequired = true;

        if (OnItemsReordered.HasDelegate)
        {
            await OnItemsReordered.InvokeAsync(_workingItems.ToList());
        }
    }

    private void OnDragEnd()
    {
        _isDragging = false;
        _dragRenderPending = true;
        _isVirtualizeRefreshRequired = true;
        ResetDragState();
    }

    private void ResetDragState()
    {
        _draggingItem = default;
        _draggingKey = null;
        _dragOverKey = null;
    }

    private object GetKey(TItem item)
    {
        if (ItemKey != null)
        {
            return ItemKey(item);
        }

        return item!;
    }

    private bool IsDraggingItem(TItem item) =>
        _isDragging && _draggingKey != null && Equals(_draggingKey, GetKey(item));

    private bool IsDropTarget(TItem item) =>
        _isDragging && _dragOverKey != null && Equals(_dragOverKey, GetKey(item)) && !Equals(_draggingKey, GetKey(item));

    private string GetDragItemClass(TItem item) =>
        IsDraggingItem(item) ? DraggingItemClass : string.Empty;

    private string GetDropTargetClass(TItem item) =>
        IsDropTarget(item) ? DropTargetItemClass : string.Empty;

    private bool SyncWorkingItems()
    {
        var incoming = Items as IReadOnlyList<TItem> ?? Items.ToList();
        var changed = incoming.Count != _workingItems.Count;

        if (!changed)
        {
            for (var i = 0; i < incoming.Count; i++)
            {
                if (!EqualityComparer<TItem>.Default.Equals(_workingItems[i], incoming[i]))
                {
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            _workingItems.Clear();
            _workingItems.AddRange(incoming);
        }

        return changed;
    }
}

