using AntDesign;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Web.Service.Contract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
namespace FileRepoSys.Web.Pages
{
    public class LoginBase:ComponentBase
    {
        [Inject]
        private IAuthenticationService _authenticationService { get; set; }
        [Inject]
        private ILocalStorageService _localStorageService { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private MessageService _messageService { get; set; }
        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; }

        public UserLoginViewModel userLoginViewModel { get; set; } = new UserLoginViewModel();

        public async void Login_Click()
        {
            var result= await _authenticationService.Login(new UserLoginViewModel { Email = userLoginViewModel.Email, Password = userLoginViewModel.Password });
            if (!result)
            {
                await _messageService.Error("用户名或密码错误");
                return;
            }
            await _messageService.Success("登录成功");
            _navigationManager.NavigateTo("/index");
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();

            if (authState.User.Identity.IsAuthenticated)
            {
                if(DateTime.Now<=DateTime.Parse(authState.User.Claims.First(c => c.Type == "expire").Value))
                {
                    await _messageService.Success("您已登录");
                    _navigationManager.NavigateTo("/index");
                }
                else
                {
                    await _localStorageService.RemoveItemsAsync(new List<string>() { "authToken" });
                    await _messageService.Success("登录信息已过期，请重新登录");
                }
            }

            await base.OnInitializedAsync();
        }

        public void GoSignup()
        {
            _navigationManager.NavigateTo("/signup");
        }
    }
}
