﻿using FileRepoSys.Api.Data;
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

        public async Task<int> AddOneFile(UserFile file, CancellationToken cancellationToken)
        {
            await _dbContext.AddAsync(file, cancellationToken);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> AddFiles(IEnumerable<UserFile> files, CancellationToken cancellationToken)
        {
            await _dbContext.UserFiles.AddRangeAsync(files);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> DeleteOneFile(Guid id, CancellationToken cancellationToken)
        {
            UserFile file = new()
            {
                Id = id,
                IsDeleted = true,
            };
            _dbContext.Entry(file).Property(file=>file.IsDeleted).IsModified=true;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateOneFile(UserFile file, CancellationToken cancellationToken)
        {
            _dbContext.Entry(file).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<UserFile> GetOneFile(Guid id, CancellationToken cancellationToken)
        {
            return _dbContext.UserFiles.SingleOrDefaultAsync(file=>file.Id == id,cancellationToken);
        }

        public async Task<List<UserFile>> GetFiles(Expression<Func<UserFile, bool>> lambda, CancellationToken cancellationToken)
        {
            return await _dbContext.UserFiles.Where(lambda).ToListAsync(cancellationToken);
        }

        public async Task<List<UserFile>> GetFilesByPage(Expression<Func<UserFile, bool>> lambda,int pageSize,int pageIndex, CancellationToken cancellationToken, bool desc=true)
        {
            if(desc)
                return await _dbContext.UserFiles.Where(lambda).OrderByDescending(file => file.CreateTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync(cancellationToken);
            else
                return await _dbContext.UserFiles.Where(lambda).OrderBy(file => file.CreateTime).Skip(pageSize*(pageIndex-1)).Take(pageSize).ToListAsync(cancellationToken);
        }

        public Task<int> GetFilesCount(Guid id)
        {
            return _dbContext.UserFiles.CountAsync(file=>file.UserId == id);
        }
    }
}
