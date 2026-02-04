using FixItUp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ===== FIX: Railway MYSQL_URL ? EF Core compatible connection string =====
var rawUrl = builder.Configuration.GetConnectionString("DefaultConnection");

// Safety check
if (string.IsNullOrWhiteSpace(rawUrl))
{
    throw new Exception("Database connection string is missing.");
}

var uri = new Uri(rawUrl);

var userInfo = uri.UserInfo.Split(':', 2);
var username = userInfo[0];
var password = userInfo.Length > 1 ? userInfo[1] : "";

var connectionString =
    $"Server={uri.Host};" +
    $"Port={uri.Port};" +
    $"Database={uri.AbsolutePath.TrimStart('/')};" +
    $"User={username};" +
    $"Password={password};" +
    $"SslMode=Required;";

// ========================================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
