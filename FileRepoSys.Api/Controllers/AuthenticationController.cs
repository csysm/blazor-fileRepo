using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Services.Contract;
using FileRepoSys.Api.Uow.Contract;
using FileRepoSys.Api.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace FileRepoSys.Api.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IUserRepository userRepository, IDistributedCache cache, IUnitOfWork unitOfWork, ILogger<AuthenticationController> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [Route("verifycode/{verifyKey}")]
        public async Task<IActionResult> GetVerifyCode(string verifyKey)
        {
            try
            {
                VerifyCode verifyCode = new VerifyCode
                {
                    SetHeight = 32,
                    SetWith = 120,
                    SetFontSize = 24
                };
                byte[] image = verifyCode.GetVerifyCodeImage();
                var anwser = verifyCode.SetVerifyCodeText;

                await _cache.SetStringAsync(verifyKey, anwser.ToLower(), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow= TimeSpan.FromSeconds(60) });
                return File(image, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromServices] IHashHelper md5Helper, [FromServices] IJWTService jwtService, [FromBody] UserLoginViewModel viewModel, CancellationToken cancellationToken)
        {
            string verifyCode=await _cache.GetStringAsync(viewModel.VerifyKey, cancellationToken);
            if (string.IsNullOrEmpty(verifyCode))
            {
                return Unauthorized("验证码已过期");
            }
            if (!verifyCode.Equals(viewModel.VerifyCode.ToLower()))
            {
                return Unauthorized("验证码错误");
            }
            try
            {
                var user = await _userRepository.FindOneUser(viewModel.Email, cancellationToken);

                var tempPass = md5Helper.MD5Encrypt32(viewModel.Password);
                if (user == null || tempPass != user.Password)
                {
                    return Unauthorized("用户名或密码错误");
                }
                if (user.IsActive == false)
                {
                    return Unauthorized("该用户未激活");
                }

                var jwtToken = jwtService.CreateToken(user, "SEMC-CJAS1-SAD-DCFDE-SAGRTYM-VF");
                return Ok(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("登录异常，请稍后再试");
            }
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromServices] ICustomMailService mailService, [FromServices] IHashHelper md5Helper, [FromBody] UserAddViewModel viewModel, CancellationToken cancellationToken)
        {
            string verifyCode=await _cache.GetStringAsync(viewModel.VerifyKey, cancellationToken);
            if (string.IsNullOrEmpty(verifyCode))
            {
                return Unauthorized("验证码已过期");
            }
            if (!verifyCode.Equals(viewModel.VerifyCode.ToLower()))
            {
                return Unauthorized("验证码错误");
            }
            if (await _userRepository.GetUsersCount() > 20)
            {
                return BadRequest("抱歉,注册用户数太多");
            }

            try
            {
                User newUser = new()
                {
                    Email = viewModel.Email,
                    Password = md5Helper.MD5Encrypt32(viewModel.Password),
                    UserName = viewModel.UserName,
                    MaxCapacity = 1024 * 1024 * 100,
                    CurrentCapacity = 0
                };
                var userId = await _userRepository.AddOneUser(newUser);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                mailService.SendActiveMail(viewModel.Email, viewModel.UserName, "http://43.140.215.157/sign-result/" + userId);
                return Ok("我们已经发送了一封激活邮件到您的邮箱，请查收");
            }
            catch (Exception)
            {

                return BadRequest("注册失败");
            }
        }

        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> Active([FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            User? user=await _userRepository.FindOneUser(userId, cancellationToken);
            if (user == null)
            {
                return BadRequest("不存在该用户");
            }
            if (user .IsDeleted==true)
            {
                return BadRequest("该用户已注销");
            }
            if (user.IsActive == true)
            {
                return BadRequest("该用户已激活");
            }

            try
            {
                user.IsActive = true;
                await _userRepository.UpdateUser(user, user => user.IsActive);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Ok("激活成功");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest("激活失败");
            }
        }
    }
}
