using AntDesign;
using Blazored.LocalStorage;
using FileRepoSys.Api.Models.UserModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FileRepoSys.Web.Pages
{
    public class IndexBase:ComponentBase
    {
        [Inject]
        private HttpClient _httpClient { get; set; }
        [Inject]
        private ILocalStorageService _localStorageService { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private MessageService _messageService { get; set; }
        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; }

        public string? UserId { get; set; }

        public UserDto userDto { get; set; }=new UserDto();

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();

            if (authState.User.Identity.IsAuthenticated)
            {
                if (DateTime.Now <= DateTime.Parse(authState.User.Claims.First(c => c.Type == "expire").Value))
                {
                    UserId = authState.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                    //var requestMessage=new HttpRequestMessage(HttpMethod.Get,$"user/{UserId}");
                    //requestMessage.Headers.Add("Authorization",$"Bearer {JWT}")
                    //var response = await _httpClient.SendAsync(requestMessage);

                    using var response = await _httpClient.GetAsync($"user/{UserId}");
                    if (response.StatusCode==System.Net.HttpStatusCode.Unauthorized)
                    {
                        await _localStorageService.RemoveItemAsync("authToken");
                        await _messageService.Error("Unauthorized");
                    }
                    if(response.StatusCode== System.Net.HttpStatusCode.NoContent)
                    {
                        await _messageService.Error("无法查询到当前用户");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        userDto = await response.Content.ReadFromJsonAsync<UserDto>();
                    }
                }
                else
                {
                    await _messageService.Warning("登录过期，请重新登录");
                    _navigationManager.NavigateTo("/");
                }
            }
            await base.OnInitializedAsync();
        }
    }
}
