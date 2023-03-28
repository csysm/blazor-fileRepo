using FileRepoSys.Api.Repository;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service;
using FileRepoSys.Api.Services.Contract;
using FileRepoSys.Api.Uow;
using FileRepoSys.Api.Uow.Contract;
using FileRepoSys.Api.Util;

namespace FileRepoSys.Api.ServiceExtension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserFileRepository, UserFileRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IHashHelper, MD5Helper>();
            services.AddScoped<IJWTService, JWTService>();

            services.AddAutoMapper(typeof(CustomeAutoMapperProfile));

            return services;
        }
    }
}
