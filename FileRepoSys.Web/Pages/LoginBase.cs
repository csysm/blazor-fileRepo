using AntDesign;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Web.Service.Contract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace FileRepoSys.Web.Pages
{
    public class LoginBase:ComponentBase
    {
        [Inject]
        private IAuthenticationService _authenticationService { get; set; }
        [Inject]
        private ILocalStorageService _localStorageService { get; set; }
        [Inject]
        IJSRuntime JS { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private MessageService _messageService { get; set; }
        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; }
        [Inject]
        private HttpClient _httpClient { get; set; }

        public UserLoginViewModel userLoginViewModel { get; set; } = new UserLoginViewModel();

        public string? VerifyCodeImg { get; set; }

        public async void Login_Click()
        {
            var result= await _authenticationService.Login(new UserLoginViewModel { Email = userLoginViewModel.Email, Password = userLoginViewModel.Password,VerifyCode=userLoginViewModel.VerifyCode,VerifyKey=await _localStorageService.GetItemAsStringAsync("login-vKey") });
            if (result.Item1)
            {
                await _messageService.Success("登录成功");
                _navigationManager.NavigateTo("index");
            }
            else
            {
                await _messageService.Error(result.Item2);
                return;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await GetVerifyCode();

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
            _navigationManager.NavigateTo("signup");
        }

        private async Task GetVerifyCode()
        {
            string verifyKey = Guid.NewGuid().ToString();
            await _localStorageService.SetItemAsStringAsync("login-vKey", verifyKey);
            var res = await _httpClient.GetAsync("authentication/verifycode/" + verifyKey);
            if (res.IsSuccessStatusCode == true)
            {
                var file = await res.Content.ReadAsByteArrayAsync();
                VerifyCodeImg = await JS.InvokeAsync<string>("byteToUrl", file);
            }
            else
            {
                await _messageService.Error("验证码获取失败");
            }
        }
    }
}
