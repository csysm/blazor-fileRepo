using AutoMapper;
using FileRepoSys.Api.Entities;
using FileRepoSys.Api.Models;
using FileRepoSys.Api.Models.UserModels;

namespace FileRepoSys.Api.Util
{
    public class CustomeAutoMapperProfile:Profile
    {
        public CustomeAutoMapperProfile()
        {
            //注册映射: CreateMap<被映射的类，映射后的类>();
            base.CreateMap<User, UserDto>();
            base.CreateMap<UserFile, UserFileDto>();
        }
    }
}
