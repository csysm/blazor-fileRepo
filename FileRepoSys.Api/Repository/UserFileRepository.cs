using FileRepoSys.Api.Data;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FileRepoSys.Api.Repository
{
    public class UserFileRepository : IUserFileRepository
    {
        private readonly FileRepoSysDbContext _dbContext;
        public UserFileRepository(FileRepoSysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddOneFile(UserFile file, CancellationToken cancellationToken=default)
        {
            await _dbContext.AddAsync(file, cancellationToken);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> DeleteOneFile(Guid id, CancellationToken cancellationToken = default)
        {
            UserFile file = new()
            {
                Id = id,
                IsDeleted = true,
            };
            _dbContext.Entry(file).Property(file=>file.IsDeleted).IsModified=true;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<UserFile> GetOneFile(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.UserFiles.SingleOrDefaultAsync(file=>file.Id == id,cancellationToken);
        }

        public async Task<List<UserFile>> GetFiles(Expression<Func<UserFile, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserFiles.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<List<UserFile>> GetFilesByPage(Expression<Func<UserFile, bool>> predicate, int pageSize,int pageIndex, CancellationToken cancellationToken = default, bool desc=true)
        {
            try
            {
                if (desc)
                    return await _dbContext.UserFiles.Where(predicate).OrderByDescending(file => file.CreateTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync(cancellationToken);
                else
                    return await _dbContext.UserFiles.Where(predicate).OrderBy(file => file.CreateTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> GetFilesCount(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.UserFiles.CountAsync(file=>file.UserId == id,cancellationToken);
        }
    }
}
