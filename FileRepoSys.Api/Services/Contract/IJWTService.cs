using FileRepoSys.Api.Entities;

namespace FileRepoSys.Api.Services.Contract
{
    public interface IJWTService
    {
        string CreateToken(User user, string securityKey);
    }
}
