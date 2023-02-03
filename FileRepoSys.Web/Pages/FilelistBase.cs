﻿using AntDesign;
using FileRepoSys.Api.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;
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
        private AuthenticationStateProvider _authenticationStateProvider { get; set; }

        [Inject]
        private MessageService _messageService { get; set; }

        [Inject] IJSRuntime JS { get; set; }

        public List<UserFileDto> userFiles { get; set; } = new List<UserFileDto>();
        public string? UserId { get; set; }
        public int PageIndex { get; set; } = 1;



        public async Task Download_ClickAsync(string fileId, string fileName)
        {
            try
            {
                var fileStream = await _httpClient.GetStreamAsync($"files/download/{fileId}");
                using var streamRef = new DotNetStreamReference(fileStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
                await _messageService.Success("下载成功");
            }
            catch (Exception)
            {
                await _messageService.Error("下载失败");
            }
        }

        public async Task Delete_ClickAsync(string fileId)
        {
            using var response = await _httpClient.DeleteAsync($"files/delete/{fileId}");
            if (response.IsSuccessStatusCode)
            {
                await _messageService.Success("删除成功");
            }
            else
            {
                await _messageService.Error("删除失败");
            }
        }

        protected override async Task OnInitializedAsync()
        {

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();

            if (authState.User.Identity.IsAuthenticated)
            {
                if (DateTime.Now <= DateTime.Parse(authState.User.Claims.First(c => c.Type == "expire").Value))
                {
                    try
                    {
                        UserId = authState.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                        userFiles = await _httpClient.GetFromJsonAsync<List<UserFileDto>>($"files?userId={UserId}&pageIndex={PageIndex}");
                    }
                    catch (Exception)
                    {

                        await _messageService.Error("请求错误,请稍后再试");
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