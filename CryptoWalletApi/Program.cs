using Microsoft.EntityFrameworkCore;
using CryptoWalletApi.Data;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var configuration = builder.Configuration;
var dbPassword = configuration["DbPassword"]; // <<< Private pw
if (string.IsNullOrEmpty(dbPassword))
{
    throw new InvalidOperationException("The database password(DbPassword) was not found in the configuration.Check user secrets or environment variables.");
}
var connectionStringBase = configuration.GetConnectionString("DbConnection"); // <<< DB
var fullConnectionString = $"{connectionStringBase};Password={dbPassword}";

builder.Services.AddDbContext<CryptoWalletDbContext>(options =>
    options.UseNpgsql(fullConnectionString)); // <<< Connect using full string

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
