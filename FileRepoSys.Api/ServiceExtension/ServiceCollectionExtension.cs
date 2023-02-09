using FileRepoSys.Api.Repository;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service;
using FileRepoSys.Api.Service.Contract;

namespace FileRepoSys.Api.ServiceExtension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddExtendServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserFileRepository, UserFileRepository>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IHashHelper, MD5Helper>();
            return services;
        }
    }
}
