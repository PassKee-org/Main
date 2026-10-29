using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using PassKee.Web.Constants.Navigation;

namespace PassKee.Web.Pages.App.Mobile;

public partial class MobileBottomNavBlock : ComponentBase
{
    [Parameter] public MobileNavigationTab ActiveTab { get; set; }
    [Parameter] public EventCallback<MobileNavigationTab> OnTabSelected { get; set; }

    private Task SelectTab(MobileNavigationTab tab) => OnTabSelected.InvokeAsync(tab);
}

