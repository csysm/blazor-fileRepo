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
                UserName = "王",
                Active = true,
                CreateTime = DateTime.Now,
                Password = "123",
                Email = "593676339@qq.com"
            });
                
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
    }
}
