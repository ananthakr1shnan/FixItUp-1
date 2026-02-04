using FixItUp.Data;
using Microsoft.EntityFrameworkCore;
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
