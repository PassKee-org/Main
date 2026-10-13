using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassKee.Web.Models.Vaults;

namespace PassKee.Web.Shared.Modals.Parts;

public partial class MoveCredentialFolderSelectBlock : MoveFolderSelectBlock
{
    private new IReadOnlyList<DecryptedDirectory> Folders => base.Folders;

    private new IReadOnlyList<(Guid? Id, string Name)> Path => base.Path;

    private new Task GoBack() => base.GoBack();
}
