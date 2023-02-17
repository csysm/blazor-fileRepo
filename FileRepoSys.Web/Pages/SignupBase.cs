using AntDesign;
using Blazored.LocalStorage;
using FileRepoSys.Api.Models.UserModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace FileRepoSys.Web.Pages
{
    public class SignupBase:ComponentBase
    {
        [Inject]
        private HttpClient _httpClient { get; set; }
        [Inject]
        private ILocalStorageService _localStorageService { get; set; }
        [Inject]
        IJSRuntime JS { get; set; }
        [Inject]
        private MessageService _msgService { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }

        public UserAddViewModel userAddViewModel { get; set; }=new UserAddViewModel();

        public string VerifyCodeImg { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetVerifyCode();
            await base.OnInitializedAsync();
        }

        public async Task Signup_Click()
        {
            userAddViewModel.VerifyKey = await _localStorageService.GetItemAsStringAsync("signup-vKey");
            var response= await _httpClient.PostAsJsonAsync("authentication/signup", userAddViewModel, CancellationToken.None);
            if (response.IsSuccessStatusCode == false)
            {
                var responseMsg= await response.Content.ReadAsStringAsync();
                await _msgService.Error(responseMsg);
            }
            else
            {
                var responseMsg = await response.Content.ReadAsStringAsync();
                await _msgService.Success(responseMsg);
            }
        }

        private async Task GetVerifyCode()
        {
            string verifyKey = Guid.NewGuid().ToString();
            await _localStorageService.SetItemAsStringAsync("signup-vKey", verifyKey);
            var res = await _httpClient.GetAsync("authentication/verifycode/" + verifyKey);
            if (res.IsSuccessStatusCode == true)
            {
                var file = await res.Content.ReadAsByteArrayAsync();
                VerifyCodeImg = await JS.InvokeAsync<string>("byteToUrl", file);
            }
            else
            {
                await _msgService.Error("验证码获取失败");
            }
        }

        public void GoLogin()
        {
            _navigationManager.NavigateTo("login");
        }
    }
}
