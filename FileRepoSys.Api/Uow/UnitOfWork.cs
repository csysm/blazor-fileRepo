using FileRepoSys.Api.Data;
using FileRepoSys.Api.Uow.Contract;

namespace FileRepoSys.Api.Uow
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly FileRepoSysDbContext _dbContext;
        public UnitOfWork(FileRepoSysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
