using FileRepoSys.Api.Data;
using FileRepoSys.Api.Repository;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Service;
using FileRepoSys.Api.Service.Contract;
using FileRepoSys.Api.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FileRepoSysDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("MySql"), MySqlServerVersion.Parse("5.7"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

//自定义服务注入
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserFileRepository, UserFileRepository>();
builder.Services.AddScoped<IFileService, FileService>();


//自定义AutoMapper
builder.Services.AddAutoMapper(typeof(CustomeAutoMapperProfile));


builder.Services.AddCors(config => config.AddPolicy("any", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SEMC-CJAS1-SAD-DCFDE-SAGRTYM-VF")),//密钥，必须与控制器中的密钥一致
                    ValidateIssuerSigningKey = true,//是否验证key
                    ValidateLifetime = true,//是否验证key是否过期
                    ValidateIssuer = false,//是否验证颁发者(一般为false)
                    ValidateAudience = false,//是否验证受众(一般为false)
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
        config =>
        {
            config.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" });

            //Swagger启用JWT鉴权

            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
                Name = "Authorization",
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
                             {
                                 {
                                     new OpenApiSecurityScheme
                                     {
                                         Reference=new OpenApiReference
                                         {
                                             Type=ReferenceType.SecurityScheme,
                                             Id="Bearer"
                                         }
                                     },
                                     new string[] {}
                                 }
                             });
            config.OperationFilter<FileUploadOperationFilter>();
        }
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("any");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
