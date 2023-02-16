using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service.Contract;
using FileRepoSys.Api.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileRepoSys.Api.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IHashHelper _md5helper;

        public AuthenticationController(IUserRepository userRepository,IMemoryCache memoryCache ,IHashHelper md5helper, ILogger<AuthenticationController> logger)
        {
            _userRepository = userRepository;
            _memoryCache = memoryCache;
            _md5helper = md5helper;
            _logger = logger;
        }

        private string CreateJWT(User user)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                //new Claim("maxCapacity",(user.MaxCapacity/1_073_741_824).ToString()),
                //new Claim("currentCapacity",(user.CurrentCapacity/1_073_741_824).ToString()),
                new Claim("expire",DateTime.Now.AddMinutes(30).ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SEMC-CJAS1-SAD-DCFDE-SAGRTYM-VF"));//密钥
            var token = new JwtSecurityToken(
                claims: claims,//将claims存储进token
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(30),//设置过期时间
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }


        [HttpGet]
        [Route("verifycode/{verifyKey}")]
        public IActionResult GetVerifyCode(string verifyKey)
        {
            try
            {
                VerifyCode verifyCode = new VerifyCode();
                verifyCode.SetHeight = 32;
                verifyCode.SetWith = 120;
                verifyCode.SetFontSize = 24;

                byte[] image = verifyCode.GetVerifyCodeImage();
                var anwser = verifyCode.SetVerifyCodeText;

                _memoryCache.Set(verifyKey, anwser.ToLower(), DateTimeOffset.Now.AddSeconds(60));
                _logger.LogInformation("获取验证码");
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
        public async Task<IActionResult> Login([FromBody] UserLoginViewModel viewModel, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(viewModel.VerifyKey))
            {
                return Unauthorized("验证码已过期");
            }
            _memoryCache.TryGetValue(viewModel.VerifyKey, out string verifyCode);
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
                var user = await _userRepository.GetOneUserByEmail(viewModel.Email, cancellationToken);

                if (user == null || _md5helper.MD5Encrypt32(viewModel.Password) != user.Password)
                {
                    return Unauthorized("用户名或密码错误");
                }
                if (user.IsActive == false)
                {
                    return Unauthorized("该用户未激活");
                }

                var jwtToken = CreateJWT(user);
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
        public async Task<IActionResult> Signup([FromBody] UserAddViewModel viewModel,CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(viewModel.VerifyKey))
            {
                return Unauthorized("验证码已过期");
            }
            _memoryCache.TryGetValue(viewModel.VerifyKey, out string verifyCode);
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
                return BadRequest("抱歉,注册用户已满");
            }

            User newUser = new()
            {
                Email = viewModel.Email,
                Password = _md5helper.MD5Encrypt32(viewModel.Password),
                UserName = viewModel.UserName,
                MaxCapacity = 1024*1024*100,
                CurrentCapacity = 0
            };

            var userId = await _userRepository.AddOneUser(newUser, cancellationToken);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("用户已经存在");
            }
            try
            {
                Mailhelper.SendMail(viewModel.Email, viewModel.UserName, "http://43.140.215.157/sign-result/"+userId);
                return Ok("我们已经发送了一封激活邮件到您的邮箱，请查收");
            }
            catch (Exception)
            {

                return BadRequest("注册失败");
            }
            
        }

        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> Active([FromQuery] string userId, CancellationToken cancellationToken)
        {
            if(Guid.TryParse(userId, out Guid id))
            {
                int result = await _userRepository.ActiveUser(id, cancellationToken);
                if (result == 0)
                {
                    return Ok("激活成功");
                }
                else if (result == 1)
                {
                    return BadRequest("不存在该用户");
                }
                else if (result == 2)
                {
                    return BadRequest("该邮箱已被注册");
                }
                else
                {
                    return BadRequest("该用户已激活");
                }
            }
            else
            {
                return BadRequest("不存在的id");
            }
        }
    }
}
