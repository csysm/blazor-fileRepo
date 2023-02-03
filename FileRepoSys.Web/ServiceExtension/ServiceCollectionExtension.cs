using FileRepoSys.Web.Service;
using FileRepoSys.Web.Service.Contract;
using FileRepoSys.Web.Util;
using Microsoft.AspNetCore.Components.Authorization;

namespace FileRepoSys.Web.ServiceExtension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }
    }
}
