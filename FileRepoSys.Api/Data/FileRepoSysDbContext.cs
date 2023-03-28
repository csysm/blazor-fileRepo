using FileRepoSys.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileRepoSys.Api.Data
{
    public class FileRepoSysDbContext:DbContext
    {
        public FileRepoSysDbContext(DbContextOptions<FileRepoSysDbContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.NewGuid(),
                UserName = "wxx",
                IsActive = true,
                MaxCapacity = 10485760,
                CurrentCapacity = 0,
                Password = "123",
                Email = "593676339@qq.com",
            });
            modelBuilder.Entity<User>().HasQueryFilter(user => user.IsDeleted == false);
            modelBuilder.Entity<UserFile>().HasQueryFilter(userFile => userFile.IsDeleted == false);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
    }
}
