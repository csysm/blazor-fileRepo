namespace FileRepoSys.Api.Services.Contract
{
    public interface ICustomMailService
    {
        void SendActiveMail(string userMail, string userName, string activeLink);
    }
}
