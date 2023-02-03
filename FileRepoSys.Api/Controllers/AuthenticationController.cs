using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models.AuthenticationModels;
using FileRepoSys.Api.Repository.Contract;
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

        private readonly IUserRepository _userRepository;

        //private readonly IMemoryCache _memoryCache;

        public AuthenticationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
                new Claim("expire",DateTime.Now.AddMinutes(1).ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SDMC-CJAS1-SAD-DFSFA-SADHJVF-VF"));//密钥
            var token = new JwtSecurityToken(
                claims: claims,//将claims存储进token
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(1),//设置过期时间
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
            if (user == null || user.Password != viewModel.Password)
            {
                return Unauthorized();
            }

            var jwtToken= CreateJWT(user);
            return Ok(jwtToken);
        }
    }
}
