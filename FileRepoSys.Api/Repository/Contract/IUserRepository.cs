using FileRepoSys.Api.Entities;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository.Contract
{
    public interface IUserRepository
    {
        Task<int> AddOneUser(User user, CancellationToken cancellationToken);
        Task<int> DeleteOneUser(Guid id, CancellationToken cancellationToken);
        Task<int> UpdateOneUser(User user, CancellationToken cancellationToken);
        Task<User> GetOneUser(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<User>> GetUsers(Expression<Func<User,bool>> lambda,CancellationToken cancellationToken);
    }
}
