namespace FileRepoSys.Api.Services.Contract
{
    public interface IFileService
    {
        Task<byte[]> GetFileAsByteArray(string filePath);
    }
}
