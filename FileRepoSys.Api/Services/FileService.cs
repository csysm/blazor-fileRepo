using FileRepoSys.Api.Services.Contract;

namespace FileRepoSys.Api.Service
{
    public class FileService:IFileService
    {
        public async Task<byte[]> GetFileAsByteArray(string filePath)
        {
            byte[] bytes=await File.ReadAllBytesAsync(filePath);
            return bytes;
        }

    }
}
