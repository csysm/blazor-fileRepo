using AntDesign;
using FileRepoSys.Api.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Models.UserFileModels;
using System.Net.Http.Json;
using System.Security.Claims;

namespace FileRepoSys.Web.Pages
{
    public class FilelistBase : ComponentBase
    {
        [Inject]
        private HttpClient _httpClient { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private AuthenticationStateProvider _authStateProvider { get; set; }
        [Inject]
        private MessageService _msgService { get; set; }
        [Inject] 
        IJSRuntime JS { get; set; }

        public string? UserId { get; set; }
        public FilelistDto fileListDto { get; set; } = new FilelistDto()
        {
            CurrentPageFiles = new List<UserFileDto>(),
            TotalCount = 0,
        };
        public int CurrentPageIndex { get; set; } = 1;

        public async Task Download_ClickAsync(string fileId, string fileName)
        {
            try
            {
                var fileStream = await _httpClient.GetStreamAsync($"files/download/{fileId}");
                using var streamRef = new DotNetStreamReference(fileStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
                await _msgService.Success("下载成功");
            }
            catch (Exception)
            {
                await _msgService.Error("下载失败");
            }
        }


        public async Task Delete_ClickAsync(string fileId)
        {
            using var response = await _httpClient.DeleteAsync($"files/delete/{fileId}");
            if (response.IsSuccessStatusCode)
            {
                await LoadData(UserId, CurrentPageIndex);
                await _msgService.Success("删除成功");
            }
            else
            {
                await _msgService.Error("删除失败");
            }
        }

        public async Task OnPageChange(PaginationEventArgs args)
        {
            await LoadData(UserId, args.Page);
            CurrentPageIndex = args.Page;
        }

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                if (DateTime.Now <= DateTime.Parse(authState.User.Claims.First(c => c.Type == "expire").Value))
                {
                    try
                    {
                        UserId = authState.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                        await LoadData(UserId,CurrentPageIndex);
                    }
                    catch (Exception)
                    {
                        await _msgService.Error("请求错误,请稍后再试");
                    }
                }
                else
                {
                    await _msgService.Warning("登录过期，请重新登录");
                    _navigationManager.NavigateTo("login");
                }
            }

            await base.OnInitializedAsync();
        }

        public async Task LoadData(string userId,int currentPageIndex)
        {
            fileListDto= await _httpClient.GetFromJsonAsync<FilelistDto>($"files?userId={userId}&pageIndex={currentPageIndex}");
        }
    }
}
