using FixItUp.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configure DbContext: prefer MYSQL_URL environment variable (for MySQL), otherwise use DefaultConnection from configuration (SQL Server)
var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
if (!string.IsNullOrEmpty(mysqlUrl))
{
    var uri = new Uri(mysqlUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;

    var connectionString =
        $"Server={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"User={username};" +
        $"Password={password};" +
        $"SslMode=Preferred;";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("No connection string configured. Set MYSQL_URL or DefaultConnection in configuration.");
    }

    // Assume DefaultConnection is for SQL Server in this environment
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAngularDev");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
