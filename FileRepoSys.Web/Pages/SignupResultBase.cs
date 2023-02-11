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
            var response = await _httpClient.GetAsync($"http://localhost:5103/authentication/active?id="+ userId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await _msgService.Success("激活成功，请登录");
                await Task.Delay(2000);
                _navigation.NavigateTo("/");
            }
            else
            {
                await _msgService.Error("激活失败,请联系管理员");
                await Task.Delay(2000);
                _navigation.NavigateTo("/");
            }

            await base.OnInitializedAsync();
        }
    }
}
