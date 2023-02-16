using Blazored.LocalStorage;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Web.Service.Contract;
using FileRepoSys.Web.Util;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FileRepoSys.Web.Service
{
    public class AuthenticationService: IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<(bool,string)> Login(UserLoginViewModel loginViewModel)
        {
            using (var respons = await _httpClient.PostAsJsonAsync("authentication/login", loginViewModel, System.Threading.CancellationToken.None))
            {
                if (!respons.IsSuccessStatusCode)
                {
                    return (false, await respons.Content.ReadAsStringAsync());
                }
                var authToken = await respons.Content.ReadAsStringAsync();
                await _localStorage.SetItemAsync("authToken", authToken);

                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginViewModel.Email);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                return (true, "登录成功");
            }
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
