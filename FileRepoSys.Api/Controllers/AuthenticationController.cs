using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Api.Models.UserModels;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service.Contract;
using FileRepoSys.Api.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileRepoSys.Api.Controllers
{
    [ApiController]
    [Route("authentication")]
    public class AuthenticationController : ControllerBase
    {
        //private readonly IMemoryCache _memoryCache;

        private readonly IUserRepository _userRepository;
        private readonly IHashHelper _md5helper;
        public AuthenticationController(IUserRepository userRepository, IHashHelper md5helper)
        {
            _userRepository = userRepository;
            _md5helper = md5helper;
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
                new Claim("expire",DateTime.Now.AddMinutes(5).ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SEMC-CJAS1-SAD-DCFDE-SAGRTYM-VF"));//密钥
            var token = new JwtSecurityToken(
                claims: claims,//将claims存储进token
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(5),//设置过期时间
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginViewModel viewModel, CancellationToken cancellationToken=default)
        {
            var user = await _userRepository.GetOneUserByEmail(viewModel.Email, cancellationToken);
            
            if (user == null || _md5helper.MD5Encrypt32(viewModel.Password) != user.Password)
            {
                return Unauthorized();
            }
            if (user.IsActive == false)
            {
                return Unauthorized("该用户未激活，请联系管理员激活");
            }

            var jwtToken= CreateJWT(user);
            return Ok(jwtToken);
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody] UserAddViewModel viewModel,CancellationToken cancellationToken = default)
        {
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
                Mailhelper.SendMail(viewModel.Email, viewModel.UserName, "http://localhost:5113/sign-result/" + userId);
                return Ok("我们已经发送了一封激活邮件到您的邮箱，请查收");
            }
            catch (Exception)
            {

                return BadRequest("注册失败");
            }
            
        }

        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> Active([FromQuery] string id, CancellationToken cancellationToken = default)
        {
            if(Guid.TryParse(id, out Guid userId))
            {
                int result = await _userRepository.ActiveUser(userId, cancellationToken);
                if (result == 0)
                {
                    return Ok("激活成功");
                }
                else if (result == 1)
                {
                    return BadRequest("激活失败，不存在该用户");
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
