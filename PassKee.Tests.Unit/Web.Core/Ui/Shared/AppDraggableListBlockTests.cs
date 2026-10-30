using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Core.Ui.Shared.Components.List;
using Xunit;

namespace PassKee.Tests.Unit.Web.Core.Ui.Shared;

[SuppressMessage("Usage", "BL0005", Justification = "Unit tests for component logic.")]
public class AppDraggableListBlockTests
{
    private record TestItem(Guid Id, string Name);

    [Fact]
    public async Task OnDropAsync_MovesDraggedItemToTargetIndexAndInvokesCallback()
    {
        var item1 = new TestItem(Guid.NewGuid(), "One");
        var item2 = new TestItem(Guid.NewGuid(), "Two");
        var item3 = new TestItem(Guid.NewGuid(), "Three");

        IList<TestItem>? reorderedResult = null;
        var component = new AppDraggableListBlock<TestItem>
        {
            Items = [item1, item2, item3],
            ItemKey = x => x.Id,
            IsDragEnabled = true,
            OnItemsReordered = EventCallback.Factory.Create<IList<TestItem>>(this, list => reorderedResult = list)
        };

        var onParametersSet = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnParametersSet", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var onDragStart = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnDragStart", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var onDragOver = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnDragOver", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var onDropAsync = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnDropAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        onParametersSet.Invoke(component, null);
        onDragStart.Invoke(component, [item3]);
        onDragOver.Invoke(component, [item1]);
        await (Task)onDropAsync.Invoke(component, [item1])!;

        Assert.NotNull(reorderedResult);
        Assert.Equal([item3.Id, item1.Id, item2.Id], reorderedResult!.Select(x => x.Id).ToArray());
    }

    [Fact]
    public async Task OnDropAsync_WhenDragDisabled_DoesNotReorder()
    {
        var item1 = new TestItem(Guid.NewGuid(), "One");
        var item2 = new TestItem(Guid.NewGuid(), "Two");

        IList<TestItem>? reorderedResult = null;
        var component = new AppDraggableListBlock<TestItem>
        {
            Items = [item1, item2],
            ItemKey = x => x.Id,
            IsDragEnabled = false,
            OnItemsReordered = EventCallback.Factory.Create<IList<TestItem>>(this, list => reorderedResult = list)
        };

        var onParametersSet = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnParametersSet", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var onDragStart = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnDragStart", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var onDropAsync = typeof(AppDraggableListBlock<TestItem>).GetMethod("OnDropAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        onParametersSet.Invoke(component, null);
        onDragStart.Invoke(component, [item2]);
        await (Task)onDropAsync.Invoke(component, [item1])!;

        Assert.Null(reorderedResult);
    }
}

