using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FileRepoSys.Web.Pages
{
    public class UserInfoBase:ComponentBase
    {
        [Inject]
        private HttpClient _httpClient { get; set; }

        [Inject]
        private NavigationManager _navigationManager { get; set; }

        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; }

        [Inject]
        private MessageService _messageService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                if (DateTime.Now <= DateTime.Parse(authState.User.Claims.First(c => c.Type == "expire").Value))
                {
                    
                }
                else
                {
                    await _messageService.Warning("登录过期，请重新登录");
                    _navigationManager.NavigateTo("login");
                }
            }

            await base.OnInitializedAsync();
        }
    }
}
