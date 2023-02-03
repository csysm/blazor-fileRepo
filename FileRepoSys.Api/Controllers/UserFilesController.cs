using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;
namespace FileRepoSys.Api.Controllers
{
    [Route("files")]
    [ApiController]
    [Authorize]
    public class UserFilesController : ControllerBase
    {
        private readonly IUserFileRepository _userFileRepopsitory;
        private readonly IUserRepository _userRepopsitory;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UserFilesController(IUserFileRepository userFileRepopsitory, IUserRepository userRepopsitory, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _userFileRepopsitory = userFileRepopsitory;
            _userRepopsitory = userRepopsitory;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserFileDto>>> Get(string userId, int pageIndex, CancellationToken cancellationToken)
        {
            var userFiles = await _userFileRepopsitory.GetFilesByPage(file => file.UserId == Guid.Parse(userId), 15, pageIndex, cancellationToken);


            var userFilesDto = _mapper.Map<List<UserFile>, List<UserFileDto>>(userFiles);
            return Ok(userFilesDto);
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Get(string userId, string keyword, int pageIndex, CancellationToken cancellationToken)
        {
            var userFiles = await _userFileRepopsitory.GetFilesByPage(file => file.UserId == Guid.Parse(userId) && file.FileName.Contains(keyword), 15, pageIndex, cancellationToken);


            var userFilesDto = _mapper.Map<List<UserFile>, List<UserFileDto>>(userFiles);
            return Ok(userFilesDto);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Add(string userId, IFormFileCollection files, CancellationToken cancellationToken = default)
        {
            if (files.Count == 0)
            {
                return BadRequest("no file");
            }

            if (files.Count > 3)
            {
                return BadRequest("can only upload 3 files once");
            }

            try
            {
                Guid currentUserId = Guid.Parse(userId);
                var currentUserEmail = User.FindFirst(ClaimTypes.Email).Value;
                var currentUser = await _userRepopsitory.GetOneUser(currentUserId, cancellationToken);
                long leftCapacity = currentUser.MaxCapacity - currentUser.CurrentCapacity;
                long uploadCapacity = 0;

                foreach (var file in files)
                {

                    uploadCapacity += file.Length;
                }

                if (uploadCapacity >= leftCapacity)
                {
                    return BadRequest("upload fail,no enough user capacity");
                }

                if (uploadCapacity > 10_485_760)
                {
                    return BadRequest("can only upload 10MB once");
                }

                string fileDirectoryPath = $"{_webHostEnvironment.ContentRootPath}/UserFiles/{currentUserEmail}";
                List<KeyValuePair<string, string>> mimeTypes = new List<KeyValuePair<string, string>>()
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
                };
                Dictionary<string, string> fileTypes = new Dictionary<string, string>(mimeTypes);

                if (!Directory.Exists(fileDirectoryPath))
                {
                    Directory.CreateDirectory(fileDirectoryPath);
                }
                
                foreach (var file in files)
                {
                    FileStream fileStream = null;

                    string suffix = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1).ToLower();//提取后缀
                    if (!fileTypes.ContainsKey(suffix))
                    {
                        return BadRequest("wrong fileType");
                    }
                    string fileName = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));
                    string fullFilePath = $"{fileDirectoryPath}/{fileName}.{suffix}";

                    //获取hash

                    FileInfo fileInfo = new FileInfo(fullFilePath);
                    fileStream = fileInfo.Create();
                    await file.CopyToAsync(fileStream);
                    fileStream.Close();
                    fileStream.Dispose();

                    UserFile userFile = new UserFile()
                    {
                        FileName = fileName,
                        FilePath = fullFilePath,
                        FileSize = file.Length,
                        FileType = fileTypes[suffix],
                        Suffix = suffix,
                        Hash = file.GetHashCode().ToString(),//
                        UserId = Guid.Parse(userId),
                    };
                    var flag = await _userFileRepopsitory.AddOneFile(userFile, cancellationToken);
                }
                

                await _userRepopsitory.UpdateUserCapacity(currentUserId, currentUser.CurrentCapacity + uploadCapacity, cancellationToken);
                return Ok("upload success");
            }
            catch (Exception)
            {
                return BadRequest("upload fail, something wrong");
            }
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

                var currentCapacity = user.CurrentCapacity -= file.FileSize;
                await _userFileRepopsitory.DeleteOneFile(userFileId, cancellationToken);
                await _userRepopsitory.UpdateUserCapacity(userId, currentCapacity, cancellationToken);
            }
            catch (Exception)
            {

                return BadRequest("delete fail");
            }
            return Ok("delete success");
        }

        [HttpGet]
        [Route("download/{fileId}")]
        public async Task<IActionResult> Get([FromServices] IFileService fileService, string fileId, CancellationToken cancellationToken = default)
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
                return Problem(ex.Message);//发布时去除
            }
            
        }
    }
}
