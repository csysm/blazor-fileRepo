using FileRepoSys.Api.Data;
using FileRepoSys.Api.ServiceExtension;
using FileRepoSys.Api.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FileRepoSysDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("MySql"), MySqlServerVersion.Parse("5.7"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

//注册自定义服务注入
builder.Services.AddExtendServices();


//自定义AutoMapper
builder.Services.AddAutoMapper(typeof(CustomeAutoMapperProfile));

//redis
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = "localhost:6379";
//    options.InstanceName = "filerepo_";
//});

//memorycache
builder.Services.AddMemoryCache();

builder.Services.AddCors(config => config.AddPolicy("localapi", p => p.WithOrigins("http://43.140.215.157/").AllowAnyMethod().AllowAnyHeader()));
//builder.Services.AddCors(config => config.AddPolicy("localtest", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

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
                    ClockSkew = TimeSpan.FromMinutes(30)
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.Urls.Add("http://localhost:5103");
app.UseCors("localapi");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
