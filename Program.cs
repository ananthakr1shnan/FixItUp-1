using FixItUp.Data;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
=======
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
if (string.IsNullOrEmpty(mysqlUrl))
{
    throw new Exception("MYSQL_URL environment variable not found");
}

var uri = new Uri(mysqlUrl);

var userInfo = uri.UserInfo.Split(':', 2);
var username = userInfo[0];
var password = userInfo.Length > 1 ? userInfo[1] : "";

var connectionString =
    $"Server={uri.Host};" +
    $"Port={uri.Port};" +
    $"Database={uri.AbsolutePath.TrimStart('/')};" +
    $"User={username};" +
    $"Password={password};" +
    $"SslMode=Preferred;";
>>>>>>> 4cafb6e63315b4a169f0b1fab8afa2ac43fa75e7

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
