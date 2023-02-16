using FileRepoSys.Api.Models.AuthenticationModels;

namespace FileRepoSys.Web.Service.Contract
{
    public interface IAuthenticationService
    {
        Task<(bool, string)> Login(UserLoginViewModel loginViewModel);
        Task Logout();
    }
}
