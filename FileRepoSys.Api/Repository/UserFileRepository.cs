using FileRepoSys.Api.Data;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository
{
    public class UserFileRepository : IUserFileRepopsitory
    {
        private readonly FileRepoSysDbContext _dbContext;
        public UserFileRepository(FileRepoSysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddOneFile(UserFile file, CancellationToken cancellationToken)
        {
            _dbContext.Entry(file).State = EntityState.Added;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> AddFiles(IEnumerable<UserFile> files, CancellationToken cancellationToken)
        {
            await _dbContext.UserFiles.AddRangeAsync(files);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteOneFile(Guid id, CancellationToken cancellationToken)
        {
            UserFile file = new() { Id = id };
            _dbContext.Entry(file).State = EntityState.Deleted;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateOneFile(UserFile file, CancellationToken cancellationToken)
        {
            _dbContext.Entry(file).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync();
        }

        public Task<UserFile> GetOneFile(Guid id, CancellationToken cancellationToken)
        {
            return _dbContext.UserFiles.SingleOrDefaultAsync(file=>file.Id == id,cancellationToken);
        }

        public async Task<IEnumerable<UserFile>> GetFiles(Expression<Func<UserFile, bool>> lambda, CancellationToken cancellationToken)
        {
            return await _dbContext.UserFiles.Where(lambda).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserFile>> GetFilesByPage(Expression<Func<UserFile, bool>> lambda, CancellationToken cancellationToken,int pageSize,int pageIndex)
        {
            return await _dbContext.UserFiles.Where(lambda).Skip(pageSize*(pageIndex-1)).Take(pageSize).ToListAsync(cancellationToken);
        }
    }
}
