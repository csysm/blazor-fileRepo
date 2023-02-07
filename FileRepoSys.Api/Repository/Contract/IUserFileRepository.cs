using FileRepoSys.Api.Entities;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository.Contract
{
    public interface IUserFileRepository
    {
        Task<int> AddOneFile(UserFile file, CancellationToken cancellationToken);
        Task<int> AddFiles(IEnumerable<UserFile> files,CancellationToken cancellationToken);
        Task<int> DeleteOneFile(Guid id, CancellationToken cancellationToken);
        Task<int> UpdateOneFile(UserFile file, CancellationToken cancellationToken);
        Task<UserFile> GetOneFile(Guid id, CancellationToken cancellationToken);
        Task<List<UserFile>> GetFiles(Expression<Func<UserFile, bool>> lambda, CancellationToken cancellationToken);
        Task<List<UserFile>> GetFilesByPage(Expression<Func<UserFile, bool>> lambda, int pageSize, int pageIndex, CancellationToken cancellationToken, bool desc = true);
        Task<int> GetFilesCount(Guid id);
    }
}
