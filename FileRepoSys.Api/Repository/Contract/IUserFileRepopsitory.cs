using FileRepoSys.Api.Entities;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository.Contract
{
    public interface IUserFileRepopsitory
    {
        Task<int> AddOneFile(UserFile file, CancellationToken cancellationToken);
        Task<int> AddFiles(IEnumerable<UserFile> files,CancellationToken cancellationToken);
        Task<int> DeleteOneFile(Guid id, CancellationToken cancellationToken);
        Task<int> UpdateOneFile(UserFile file, CancellationToken cancellationToken);
        Task<UserFile> GetOneFile(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<UserFile>> GetFiles(Expression<Func<UserFile, bool>> lambda, CancellationToken cancellationToken);
        Task<IEnumerable<UserFile>> GetFilesByPage(Expression<Func<UserFile, bool>> lambda, CancellationToken cancellationToken, int pageSize, int pageIndex, bool desc = true);
    }
}
