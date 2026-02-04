using FixItUp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var raw = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(raw))
{
    throw new Exception("DefaultConnection is missing. Check Railway variables.");
}

string connectionString;

// CASE 1: Railway-style mysql:// URL
if (raw.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(raw);

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    connectionString =
        $"Server={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"User={username};" +
        $"Password={password};" +
        $"SslMode=Required;";
}
else
{
    // CASE 2: Already a normal MySQL connection string
    connectionString = raw;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
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
