using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Models.UserFileModels;
using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace FileRepoSys.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    [Authorize]
    public class UserFilesController : ControllerBase
    {
        private readonly IUserFileRepository _userFileRepopsitory;
        private readonly IUserRepository _userRepopsitory;
        private readonly IMapper _mapper;
        private readonly ILogger<UserFilesController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UserFilesController(IUserFileRepository userFileRepopsitory, IUserRepository userRepopsitory, IMapper mapper, IWebHostEnvironment webHostEnvironment, ILogger<UserFilesController> logger)
        {
            _userFileRepopsitory = userFileRepopsitory;
            _userRepopsitory = userRepopsitory;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<FilelistDto>> Get(string userId, int pageIndex, CancellationToken cancellationToken)
        {
            Guid userIdGuid = Guid.Empty;
            if (!Guid.TryParse(userId,out userIdGuid))
            {
                return BadRequest("不存在的id");
            }

            try
            {
                var userFiles = await _userFileRepopsitory.GetFilesByPage(file => file.UserId == Guid.Parse(userId), 15, pageIndex, cancellationToken);
                int count = await _userFileRepopsitory.GetFilesCount(Guid.Parse(userId));
                var userFileDtos = _mapper.Map<List<UserFile>, List<UserFileDto>>(userFiles);

                FilelistDto fileListDto = new();
                fileListDto.CurrentPageFiles = userFileDtos;
                fileListDto.TotalCount = count;

                return Ok(fileListDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("something wrong");
            }
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Get(string userId, string keyword, int pageIndex, CancellationToken cancellationToken)
        {
            Guid userIdGuid = Guid.Empty;
            if (!Guid.TryParse(userId, out userIdGuid))
            {
                return BadRequest("不存在的id");
            }

            try
            {
                var userFiles = await _userFileRepopsitory.GetFilesByPage(file => file.UserId == Guid.Parse(userId) && file.FileName.Contains(keyword), 15, pageIndex, cancellationToken);

                var userFilesDto = _mapper.Map<List<UserFile>, List<UserFileDto>>(userFiles);
                return Ok(userFilesDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("something wrong");
            }
        }

        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult<IList<UploadResult>>> Post([FromForm] IEnumerable<IFormFile> files, CancellationToken cancellationToken)
        {
            int maxAllowedFiles = 3;//单次上传最多3个文件
            long maxFileSize = 1024 * 1024 * 10;//单个文件最大长度10MB
            int filesProcessed = 0;//已经处理的文件数量
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
            List<UploadResult> uploadResults = new();

            Guid currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentUser = await _userRepopsitory.GetOneUser(currentUserId, cancellationToken);
            long leftCapacity = currentUser.MaxCapacity - currentUser.CurrentCapacity;
            long uploadedCapacity = 0;

            Dictionary<string, string> fileTypes = new Dictionary<string, string>(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string,string>("txt",System.Net.Mime.MediaTypeNames.Text.Plain),
                    new KeyValuePair<string,string>("html",System.Net.Mime.MediaTypeNames.Text.Html),
                    new KeyValuePair<string,string>("gif",System.Net.Mime.MediaTypeNames.Image.Gif),
                    new KeyValuePair<string,string>("png","image/png"),
                    new KeyValuePair<string,string>("jpeg","image/jpeg"),
                    new KeyValuePair<string,string>("jpg","image/jpeg"),
                    new KeyValuePair<string,string>("pdf","application/pdf"),
                    new KeyValuePair<string,string>("doc","application/msword"),
                    new KeyValuePair<string,string>("docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
                    new KeyValuePair<string,string>("xls","application/vnd.ms-excel"),
                    new KeyValuePair<string,string>("xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                    new KeyValuePair<string,string>("mp3","audio/mpeg"),
                    new KeyValuePair<string,string>("zip","application/zip"),
                    new KeyValuePair<string,string>("rar","application/x-rar-compressed"),
                    new KeyValuePair<string,string>("md","text/markdown"),
                    new KeyValuePair<string,string>("json","application/json")
                });

            var fileDirectoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserFiles", currentUser.Email);
            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }

            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;
                uploadResult.FileName = untrustedFileName;
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);
                

                string suffix = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1).ToLower();//提取后缀
                string fileName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));//提取文件名

                uploadedCapacity += file.Length;

                if (filesProcessed < maxAllowedFiles)
                {
                    if (file.Length == 0)
                    {
                        uploadedCapacity -= file.Length;
                        _logger.LogInformation("{FileName} 长度为 0 (Err: 1)", trustedFileNameForDisplay);
                        uploadResult.ErrorCode = 1;
                    }
                    else if (file.Length > maxFileSize)
                    {
                        uploadedCapacity -= file.Length;
                        _logger.LogInformation("{FileName} 的长度 {Length} bytes 超过了 {Limit} bytes 的限制  (Err: 2)", trustedFileNameForDisplay, file.Length, maxFileSize);
                        uploadResult.ErrorCode = 2;
                    }
                    else if (!fileTypes.ContainsKey(suffix))
                    {
                        uploadedCapacity -= file.Length;
                        _logger.LogInformation("contains illegal file");
                        uploadResult.ErrorCode = 8;
                        break;
                    }
                    else if (uploadedCapacity > leftCapacity)
                    {
                        uploadedCapacity -= file.Length;
                        _logger.LogInformation("account capacity is not enough ");
                        uploadResult.ErrorCode = 7;
                        break;
                    }
                    else
                    {
                        try
                        {
                            trustedFileNameForFileStorage = Path.GetRandomFileName();

                            var fileFullPath = Path.Combine(fileDirectoryPath, trustedFileNameForFileStorage);

                            await using FileStream fs = new(fileFullPath, FileMode.Create);
                            await file.CopyToAsync(fs);

                            UserFile userFile = new UserFile()
                            {
                                FileName = fileName,
                                FileStorageName = trustedFileNameForFileStorage,
                                FilePath = fileFullPath,
                                FileSize = file.Length,
                                MimeType = fileTypes[suffix],
                                Suffix = suffix,
                                UserId = currentUser.Id
                            };
                            await _userFileRepopsitory.AddOneFile(userFile, cancellationToken);

                            _logger.LogInformation("{FileName} saved at {Path}", trustedFileNameForDisplay, fileFullPath);
                            uploadResult.Uploaded = true;
                            uploadResult.StoredFileName = trustedFileNameForFileStorage;
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError("{FileName} error on upload (Err: 3): {Message}", trustedFileNameForDisplay, ex.Message);
                            uploadResult.ErrorCode = 3;
                        }
                    }
                    filesProcessed++;
                }
                else
                {
                    _logger.LogInformation("{FileName} not uploaded because the " + "上传文件超出 {Count} 个 (Err: 4)", trustedFileNameForDisplay, maxAllowedFiles);
                    uploadResult.ErrorCode = 4;
                }

                uploadResults.Add(uploadResult);
            }
            if(uploadedCapacity>0)
            {
                await _userRepopsitory.UpdateUserCapacity(currentUserId, currentUser.CurrentCapacity + uploadedCapacity, cancellationToken);
            }
            return new CreatedResult(resourcePath, uploadResults);
        }

        [HttpDelete]
        [Route("delete/{fileId}")]
        public async Task<IActionResult> Delete(string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var userFileId = Guid.Parse(fileId);

                var file = await _userFileRepopsitory.GetOneFile(userFileId, cancellationToken);
                var user = await _userRepopsitory.GetOneUser(userId, cancellationToken);

                if (file == null)
                {
                    return BadRequest("file is not exist");
                }

                await _userFileRepopsitory.DeleteOneFile(userFileId, cancellationToken);
                System.IO.File.Delete(file.FilePath);
                var currentCapacity = user.CurrentCapacity -= file.FileSize;
                await _userRepopsitory.UpdateUserCapacity(userId, currentCapacity, cancellationToken);
                return Ok("delete success");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("delete fail");
            }
        }

        [HttpGet]
        [Route("download/{fileId}")]
        public async Task<IActionResult> Get([FromServices] IFileService fileService, string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var file = await _userFileRepopsitory.GetOneFile(Guid.Parse(fileId), cancellationToken);
                //var bytes = await fileService.GetFileAsByteArray(file.FilePath);
                //return File(bytes, file.FileType, file.FileName);
                Stream fileStream = System.IO.File.Open(file.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(fileStream, "application/octet-stream", file.FileName);
                fileStream.Close();
                fileStream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("下载失败");
            }
        }
    }
}
