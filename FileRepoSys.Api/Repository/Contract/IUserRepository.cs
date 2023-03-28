using FileRepoSys.Api.Entities;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository.Contract
{
    public interface IUserRepository
    {
        Task<Guid> AddOneUser(User user);

        Task DeleteOneUser(Guid id);

        Task UpdateUser(User user);

        Task UpdateUser(User user, params Expression<Func<User, object>>[] properties);

        Task<User> GetOneUser(Guid id, CancellationToken cancellationToken = default);

        Task<User> GetOneUser(string email,CancellationToken cancellationToken = default);

        Task<User?> FindOneUser(Guid id, CancellationToken cancellationToken = default);

        Task<User?> FindOneUser(string email, CancellationToken cancellationToken = default);

        Task<List<User>> GetUsers(Expression<Func<User,bool>> predicate, CancellationToken cancellationToken = default);

        Task<List<User>> GetUsersByPage(Expression<Func<User, bool>> predicate, int pageSize, int pageIndex, CancellationToken cancellationToken = default, bool desc = true);

        Task<int> GetUsersCount(CancellationToken cancellationToken = default);
    }
}
