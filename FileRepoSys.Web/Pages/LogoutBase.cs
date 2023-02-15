using FileRepoSys.Web.Service.Contract;
using Microsoft.AspNetCore.Components;

namespace FileRepoSys.Web.Pages
{
    public class LogoutBase:ComponentBase
    {
        [Inject]
        public IAuthenticationService AuthService { get; set; }
        [Inject] 
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await AuthService.Logout();
            NavigationManager.NavigateTo("login");
        }
    }
}
