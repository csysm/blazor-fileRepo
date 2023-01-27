using FileRepoSys.Api.Data;
using FileRepoSys.Api.Repository;
using FileRepoSys.Api.Repository.Contract;
using FileRepoSys.Api.Util;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FileRepoSysDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("MySql"), MySqlServerVersion.Parse("5.7"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});


builder.Services.AddScoped<IUserRepository, UserRepository>();

//×Ô¶¨ÒåAutoMapper
builder.Services.AddAutoMapper(typeof(CustomeAutoMapperProfile));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
