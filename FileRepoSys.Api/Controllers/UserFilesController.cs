using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models;
using FileRepoSys.Api.Repository.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Models.UserFileModels;
using System.Net;
using FileRepoSys.Api.Services.Contract;
using FileRepoSys.Api.Uow.Contract;

namespace FileRepoSys.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    [Authorize]
    public class UserFilesController : ControllerBase
    {
        private readonly IUserFileRepository _userFileRepopsitory;
        private readonly IUserRepository _userRepopsitory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserFilesController> _logger;

        public UserFilesController(IUserFileRepository userFileRepopsitory, IUserRepository userRepopsitory, IMapper mapper, ILogger<UserFilesController> logger, IUnitOfWork unitOfWork)
        {
            _userFileRepopsitory = userFileRepopsitory;
            _userRepopsitory = userRepopsitory;

            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<FilelistDto>> Get([FromServices] IMapper _mapper, Guid userId, int pageIndex, CancellationToken cancellationToken)
        {
            try
            {
                var userFileList = await _userFileRepopsitory.GetFilesByPage(file => file.UserId == userId, 15, pageIndex, cancellationToken);
                int userFileCount = await _userFileRepopsitory.GetFilesCount(userId);
                var userFileDtoList = _mapper.Map<List<UserFile>, List<UserFileDto>>(userFileList);

                FilelistDto filePageDto = new();
                filePageDto.CurrentPageFiles = userFileDtoList;
                filePageDto.TotalCount = userFileCount;

                return Ok(filePageDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("something wrong");
            }
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Get([FromServices] IMapper _mapper, Guid userId, string keyword, int pageIndex, CancellationToken cancellationToken)
        {
            try
            {
                var userFileList = await _userFileRepopsitory.GetFilesByPage(file => file.UserId == userId && file.FileName.Contains(keyword), 15, pageIndex, cancellationToken);
                var userFileDtoList = _mapper.Map<List<UserFile>, List<UserFileDto>>(userFileList);

                FilelistDto filePageDto = new();
                filePageDto.CurrentPageFiles = userFileDtoList;
                filePageDto.TotalCount = userFileDtoList.Count;

                return Ok(filePageDto);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("something wrong");
            }
        }

        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult<IList<UploadResult>>> Post([FromServices] IWebHostEnvironment _webHostEnvironment, [FromForm] IEnumerable<IFormFile> files, CancellationToken cancellationToken)
        {
            Guid userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var user = await _userRepopsitory.GetOneUser(userId, cancellationToken);
            long leftCapacity = user.MaxCapacity - user.CurrentCapacity;

            if (leftCapacity < files.Sum(f => f.Length))
            {
                _logger.LogInformation("账户容量不足");
                return BadRequest("账户容量不足");
            }

            const int maxAllowedFiles = 3;  //单次上传最多3个文件
            const long maxFileSize = 1024 * 1024 * 10;  //单个文件最大长度10MB
            int filesProcessed = 0; //已经处理的文件数量

            var uploadResults = new List<UploadResult>();

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
                    new KeyValuePair<string,string>("ppt","application/vnd.ms-powerpoint"),
                    new KeyValuePair<string,string>("pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"),
                    new KeyValuePair<string,string>("mp3","audio/mpeg"),
                    new KeyValuePair<string,string>("zip","application/zip"),
                    new KeyValuePair<string,string>("rar","application/x-rar-compressed"),
                    new KeyValuePair<string,string>("md","text/markdown"),
                    new KeyValuePair<string,string>("json","application/json")
                });

            var fileDirectoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserFiles", user.Email);
            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }

            foreach (var file in files)
            {
                filesProcessed++;

                var uploadResult = new UploadResult();
                uploadResult.FileName = file.FileName;

                string fileName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));//提取文件名
                string suffix = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1).ToLower();//提取后缀

                if (filesProcessed <= maxAllowedFiles)
                {
                    if (file.Length == 0 || file.Length > maxFileSize)
                    {
                        _logger.LogInformation($"{fileName} 文件大小为有误 (ERR:1)");
                        uploadResult.ErrorCode = 1;
                    }
                    else if (fileName.Length<3|| fileName.Length>64)
                    {
                        _logger.LogInformation($"{fileName} 文件名长度有误 (ERR:2)");
                        uploadResult.ErrorCode = 2;
                    }
                    else if (!fileTypes.ContainsKey(suffix))
                    {
                        _logger.LogInformation("非法文件类型");
                        uploadResult.ErrorCode = 3;
                    }
                    else
                    {
                        try
                        {
                            string trustedFileStorageName = Path.GetRandomFileName();
                            string fileFullPath = Path.Combine(fileDirectoryPath, trustedFileStorageName);

                            await using FileStream fs = new(fileFullPath, FileMode.Create);
                            await file.CopyToAsync(fs);

                            UserFile userFile = new UserFile()
                            {
                                FileName = fileName,
                                FileStorageName = trustedFileStorageName,
                                FilePath = fileFullPath,
                                FileSize = file.Length,
                                MimeType = fileTypes[suffix],
                                Suffix = suffix,
                                UserId = user.Id
                            };
                            await _userFileRepopsitory.AddOneFile(userFile, cancellationToken);

                            uploadResult.Uploaded = true;
                            uploadResult.StoredFileName = trustedFileStorageName;
                            _logger.LogInformation($"上传文件 {fileName} 成功 {fileFullPath}");

                            uploadedCapacity += file.Length;
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError($"上传文件 {fileName} 错误 (ERR: 3): {ex.Message}");
                            uploadResult.ErrorCode = 4;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"{fileName} 未上传该文件,上传文件超出 {maxAllowedFiles} 个 (ERR: 4)");
                    uploadResult.ErrorCode = 5;
                }
                uploadResults.Add(uploadResult);
            }

            if (uploadedCapacity > 0)
            {
                user.CurrentCapacity += uploadedCapacity;
                await _userRepopsitory.UpdateUser(user, user => user.CurrentCapacity);
                await _unitOfWork.SaveChangesAsync();
            }

            return uploadResults;
        }

        [HttpDelete]
        [Route("delete/{fileId}")]
        public async Task<IActionResult> Delete(Guid fileId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

                var user = await _userRepopsitory.GetOneUser(userId, cancellationToken);
                var file = await _userFileRepopsitory.GetOneFile(fileId, cancellationToken);

                await _userFileRepopsitory.DeleteOneFile(fileId);
                System.IO.File.Delete(file.FilePath);

                user.CurrentCapacity -= file.FileSize;
                await _userRepopsitory.UpdateUser(user, user => user.CurrentCapacity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Ok("delete success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("delete fail");
            }
        }

        [HttpGet]
        [Route("download/{fileId}")]
        public async Task<IActionResult> Get([FromServices] IFileService fileService, Guid fileId, CancellationToken cancellationToken)
        {
            try
            {
                var file = await _userFileRepopsitory.GetOneFile(fileId, cancellationToken);
                //var bytes = await fileService.GetFileAsByteArray(file.FilePath);
                //return File(bytes, file.FileType, file.FileName);
                Stream fileStream = System.IO.File.Open(file.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(fileStream, "application/octet-stream", file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("下载失败");
            }
        }
    }
}
