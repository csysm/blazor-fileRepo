using Blazored.LocalStorage;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Web.Service.Contract;
using FileRepoSys.Web.Util;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

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

        public async Task<bool> Login(UserLoginViewModel loginViewModel)
        {
            //var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            //{
            //    new(nameof(UserLoginViewModel.Email), loginViewModel.Email),
            //    new(nameof(UserLoginViewModel.Password), loginViewModel.Password),
            //});
            using (var respons = await _httpClient.PostAsJsonAsync("authentication/login", loginViewModel, System.Threading.CancellationToken.None))
            {
                if (!respons.IsSuccessStatusCode)
                {
                    return false;
                }
                var authToken = await respons.Content.ReadAsStringAsync();
                await _localStorage.SetItemAsync("authToken", authToken);

                //var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
                //var username = state.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginViewModel.Email);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                return true;
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
