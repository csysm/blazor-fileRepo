using FileRepoSys.Api.Entities;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository.Contract
{
    public interface IUserFileRepository
    {
        Task<int> AddOneFile(UserFile file, CancellationToken cancellationToken = default);

        Task<int> DeleteOneFile(Guid id, CancellationToken cancellationToken = default);

        Task<UserFile> GetOneFile(Guid id, CancellationToken cancellationToken = default);

        Task<List<UserFile>> GetFiles(Expression<Func<UserFile, bool>> predicate, CancellationToken cancellationToken = default);

        Task<List<UserFile>> GetFilesByPage(Expression<Func<UserFile, bool>> predicate, int pageSize, int pageIndex, CancellationToken cancellationToken = default, bool desc = true);

        Task<int> GetFilesCount(Guid id, CancellationToken cancellationToken = default);
    }
}
