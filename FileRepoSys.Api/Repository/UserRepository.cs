using FileRepoSys.Api.Data;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Util;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly FileRepoSysDbContext _dbContext;

        public UserRepository(FileRepoSysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private Task<bool> IsUserExistAsync(string email)
        {
            return _dbContext.Users.IgnoreQueryFilters().AnyAsync(user => user.Email == email);
        }

        public async Task<Guid> AddOneUser(User user)
        {
            if(await IsUserExistAsync(user.Email))
            {
                throw new Exception("该邮箱已注册");
            }
            await _dbContext.AddAsync(user);
            return user.Id;//返回自动生成的主键值
        }

        public Task DeleteOneUser(Guid id)
        {
            User user = new()
            {
                Id = id,
                IsDeleted = true
            };
            _dbContext.Entry(user).Property(user=>user.IsDeleted).IsModified=true;
            return Task.CompletedTask;
        }

        public Task UpdateUser(User user)
        {
            _dbContext.Update(user);
            return Task.CompletedTask;
        }

        public Task UpdateUser(User user, params Expression<Func<User, object>>[] properties)
        {
            _dbContext.Attach(user);
            foreach (var property in properties)
            {
                _dbContext.Entry(user).Property(property).IsModified = true;
            }
            return Task.CompletedTask;
        }

        public Task<User> GetOneUser(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.SingleAsync(user => user.Id == id,cancellationToken);
        }

        public Task<User> GetOneUser(string email, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.SingleAsync(user => user.Email == email, cancellationToken);
        }

        public Task<User?> FindOneUser(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public Task<User?> FindOneUser(string email, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<List<User>> GetUsers(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<List<User>> GetUsersByPage(Expression<Func<User, bool>> predicate, int pageSize, int pageIndex,  CancellationToken cancellationToken = default, bool desc = true)
        {
            if (desc)
                return await _dbContext.Users.Where(predicate).OrderByDescending(user => user.CreateTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync(cancellationToken);
            else
                return await _dbContext.Users.Where(predicate).OrderBy(user => user.CreateTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync(cancellationToken);
        }

        public Task<int> GetUsersCount(CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.IgnoreQueryFilters().CountAsync(cancellationToken);
        }
    }
}
