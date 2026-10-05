using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Pages.App.Parts;

public partial class SearchMatchingFoldersBlock : ComponentBase
{
    [Parameter]
    public IReadOnlyList<DecryptedDirectory> Directories { get; set; } = [];

    [Parameter]
    public IReadOnlyDictionary<Guid, string> DirectoryPaths { get; set; } = new Dictionary<Guid, string>();

    [Parameter]
    public EventCallback<Guid?> OnSelectDirectory { get; set; }

    [Parameter]
    public Func<Guid, int> GetItemCount { get; set; } = _ => 0;

    private string GetDirectoryPath(Guid dirId) =>
        DirectoryPaths.TryGetValue(dirId, out var path) ? path : string.Empty;
}
