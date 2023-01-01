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

        public async Task<int> AddOneUser(User user,CancellationToken cancellationToken)
        {
            _dbContext.Entry(user).State = EntityState.Added;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> DeleteOneUser(Guid id, CancellationToken cancellationToken)
        {
            User user = new(){ Id = id };
            _dbContext.Entry(user).State = EntityState.Deleted;
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
