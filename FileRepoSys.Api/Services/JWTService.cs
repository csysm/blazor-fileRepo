using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Services.Contract;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileRepoSys.Api.Service
{
    public class JWTService:IJWTService
    {
        public string CreateToken(User user,string securityKey)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("expire",DateTime.Now.AddMinutes(30).ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));//密钥
            var token = new JwtSecurityToken(
                claims: claims,//将claims存储进token
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(30),//设置过期时间
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }
    }
}
