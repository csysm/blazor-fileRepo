using AntDesign;
using Microsoft.AspNetCore.Components;

namespace FileRepoSys.Web.Pages
{
    public class SignupResultBase:ComponentBase
    {
        [Inject]
        private  HttpClient _httpClient { get; set; }
        [Inject]
        private NavigationManager _navigation { get; set; }
        [Inject]
        private MessageService _msgService { get; set; }
        [Parameter]
        public string userId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var response = await _httpClient.GetAsync("authentication/active?userId=" + userId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await _msgService.Success(await response.Content.ReadAsStringAsync());
                await Task.Delay(2000);
                _navigation.NavigateTo("login");
            }
            else
            {
                await _msgService.Error(await response.Content.ReadAsStringAsync());
                await Task.Delay(2000);
                _navigation.NavigateTo("login");
            }

            await base.OnInitializedAsync();
        }
    }
}
