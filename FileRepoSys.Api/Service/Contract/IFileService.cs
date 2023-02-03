namespace FileRepoSys.Api.Service.Contract
{
    public interface IFileService
    {
        Task<byte[]> GetFileAsByteArray(string filePath);
    }
}
