using FileRepoSys.Api.Entities;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository.Contract
{
    public interface IUserRepository
    {
        Task<string> AddOneUser(User user, CancellationToken cancellationToken = default);

        Task<int> DeleteOneUser(Guid id, CancellationToken cancellationToken = default);

        Task<int> UpdateUser(User user, CancellationToken cancellationToken = default);

        Task<int> ActiveUser(Guid id, CancellationToken cancellationToken = default);

        Task<User> GetOneUser(Guid id, CancellationToken cancellationToken = default);

        Task<User> GetOneUserByEmail(string email,CancellationToken cancellationToken = default);

        Task<List<User>> GetUsers(Expression<Func<User,bool>> lambda,CancellationToken cancellationToken = default);

        Task<List<User>> GetUsersByPage(Expression<Func<User, bool>> lambda, int pageSize, int pageIndex, CancellationToken cancellationToken = default, bool desc = true);

        Task<int> UpdateUserName(User user, CancellationToken cancellationToken = default);

        Task<int> UpdateUserPassword(string oldPassword, User user,  CancellationToken cancellationToken = default);

        Task<int> UpdateUserCapacity(Guid id, long currentCapacity, CancellationToken cancellationToken = default);
    }
}
