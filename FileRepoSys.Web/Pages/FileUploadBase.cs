using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using FileRepoSys.Web.Util;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using AntDesign;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FileRepoSys.Web.Pages
{
    public class FileUploadBase : ComponentBase
    {
        [Inject]
        private HttpClient Http { get; set; }

        [Inject]
        public ILogger<FileUpload> Logger { get; set; }

        [Inject]
        public  MessageService _messageService { get; set; }

        [Inject]
        private NavigationManager _navigationManager { get; set; }

        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; }

        public List<File> files = new();
        public List<UploadResult> uploadResults = new();
        public int maxAllowedFiles = 3;
        public bool shouldRender;

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                if (DateTime.Now <= DateTime.Parse(authState.User.Claims.First(c => c.Type == "expire").Value))
                {

                }
                else
                {
                    await _messageService.Warning("登录过期，请重新登录");
                    _navigationManager.NavigateTo("login");
                }
            }
            await base.OnInitializedAsync();
        }

        protected override bool ShouldRender() => shouldRender;

        public async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            shouldRender = false;
            long maxFileSize = 1024 * 1024 * 10;
            var upload = false;

            using var content = new MultipartFormDataContent();

            if (e.FileCount > 3)
            {
                await _messageService.Warning("单次上传的文件数不能超过3");
                return;
            }

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles))
            {
                if (file.Name.Substring(0, file.Name.LastIndexOf('.')).Length <= 2)
                {
                    await _messageService.Warning("文件名长度不能小于2");
                    return;
                }
                if (file.Name.Substring(0, file.Name.LastIndexOf('.')).Length > 32)
                {
                    await _messageService.Warning("文件名长度不能大于32");
                    return;
                }
                if (file.Size>maxFileSize)
                {
                    await _messageService.Warning("单个文件不能大于10MB");
                    return;
                }
                if (uploadResults.SingleOrDefault(f => f.FileName == file.Name) is null)
                {
                    try
                    {
                        files.Add(new() { Name = file.Name });

                        var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));

                        if (string.IsNullOrEmpty(file.ContentType))
                        {
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        }
                        else
                        {
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        }

                        content.Add(content: fileContent, name: "\"files\"", fileName: file.Name);

                        upload = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInformation("{FileName} not uploaded (Err: 6): {Message}", file.Name, ex.Message);

                        uploadResults.Add(new()
                        {
                            FileName = file.Name,
                            ErrorCode = 6,
                            Uploaded = false
                        });
                    }
                }
            }

            if (upload)
            {
                var response = await Http.PostAsync("files/upload", content);

                var newUploadResults = await response.Content.ReadFromJsonAsync<IList<UploadResult>>();

                if (newUploadResults is not null)
                {
                    uploadResults = uploadResults.Concat(newUploadResults).ToList();
                }
            }

            shouldRender = true;
        }

        public static bool FileUpload(IList<UploadResult> uploadResults, string? fileName, ILogger<FileUpload> logger, out UploadResult result)
        {
            result = uploadResults.SingleOrDefault(f => f.FileName == fileName) ?? new();

            if (!result.Uploaded)
            {
                logger.LogInformation("{FileName} 未上传成功 (Err: 5)", fileName);
                result.ErrorCode = 5;
            }

            return result.Uploaded;
        }

        public class File
        {
            public string? Name { get; set; }
        }
    }
}
