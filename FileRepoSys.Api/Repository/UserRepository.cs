using FileRepoSys.Api.Data;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Repository.Contract;
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
            return _dbContext.Users.AnyAsync(user => user.Email == email);
        }

        public async Task<int> AddOneUser(User user,CancellationToken cancellationToken)
        {
            if(await IsUserExistAsync(user.Email))
            {
                return 0;
            }
            _dbContext.Entry(user).State = EntityState.Added;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> DeleteOneUser(Guid id, CancellationToken cancellationToken)
        {
            User user = await GetOneUser(id, cancellationToken);
            user.IsDeleted = true;
            _dbContext.Entry(user).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateUser(User user, CancellationToken cancellationToken)
        {
            _dbContext.Entry(user).State= EntityState.Modified;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateUserName(User user,CancellationToken cancellationToken)
        {
            _dbContext.Entry(user).Property(user=>user.UserName).IsModified= true;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateUserPassword(string oldPassword,User user, CancellationToken cancellationToken)
        {
            bool isExisted= await _dbContext.Users.AnyAsync(u => u.Id == user.Id && u.Password == oldPassword);
            if (isExisted == false)//若用户不存在
            {
                return 0;
            }
            _dbContext.Entry(user).Property(user => user.Password).IsModified = true;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> ActiveUser(Guid id, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == id);
            if(user == null||user.IsDeleted==true)
            {
                return 0;
            }
            user.IsActive = true;
            _dbContext.Entry(user).Property(user => user.IsActive).IsModified = true;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        

        public Task<User> GetOneUser(Guid id, CancellationToken cancellationToken)
        {
            return _dbContext.Users.SingleOrDefaultAsync(user => user.Id == id,cancellationToken);
        }

        public async Task<List<User>> GetUsers(Expression<Func<User, bool>> lambda, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.Where(lambda).ToListAsync(cancellationToken);
        }

        public async Task<List<User>> GetUsersByPage(Expression<Func<User, bool>> lambda, int pageSize, int pageIndex,  CancellationToken cancellationToken,bool desc = true)
        {
            if (desc)
                return await _dbContext.Users.Where(lambda).Skip(pageSize * (pageIndex - 1)).Take(pageSize).OrderByDescending(user => user.CreateTime).ToListAsync(cancellationToken);
            else
                return await _dbContext.Users.Where(lambda).Skip(pageSize * (pageIndex - 1)).Take(pageSize).OrderBy(user => user.CreateTime).ToListAsync(cancellationToken);
        }
    }
}
