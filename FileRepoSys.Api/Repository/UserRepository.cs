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

        public async Task<int> UpdateOneUser(User user, CancellationToken cancellationToken)
        {
            _dbContext.Entry(user).State= EntityState.Modified;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<User> GetOneUser(Guid id, CancellationToken cancellationToken)
        {
            return _dbContext.Users.SingleOrDefaultAsync(user => user.Id == id,cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsers(Expression<Func<User, bool>> lambda, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.Where(lambda).ToListAsync(cancellationToken);
        }
    }
}
