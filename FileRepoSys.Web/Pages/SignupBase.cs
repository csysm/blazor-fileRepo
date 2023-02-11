using AntDesign;
using FileRepoSys.Api.Models.UserModels;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace FileRepoSys.Web.Pages
{
    public class SignupBase:ComponentBase
    {
        [Inject]
        private HttpClient _httpClient { get; set; }

        [Inject]
        private MessageService _msgService { get; set; }

        public UserAddViewModel userAddViewModel { get; set; }=new UserAddViewModel();

        protected override async Task OnInitializedAsync()
        {


            await base.OnInitializedAsync();
        }

        public async Task Signup_Click()
        {
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
    }
}
