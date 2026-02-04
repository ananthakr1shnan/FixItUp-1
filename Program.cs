using FixItUp.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL")
    ?? throw new Exception("MYSQL_URL environment variable not found");

var csb = new MySqlConnectionStringBuilder(mysqlUrl);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        csb.ConnectionString,
        ServerVersion.AutoDetect(csb.ConnectionString)
    )
);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
