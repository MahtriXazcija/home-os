using HomeOS.Infrastructure.Identity;
using HomeOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render (and similar PaaS hosts) assign the listen port via $PORT at runtime.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

const string CorsPolicy = "HomeOsCors";

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Comma-separated list in config/env var "Cors:AllowedOrigins", falling back to
// the local Vite dev server. Add the production frontend URL there once deployed
// — no code change needed.
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("HomeOsDb");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<HomeOsDbContext>(options => options.UseNpgsql(connectionString));
    builder.Services
        .AddIdentityCore<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<HomeOsDbContext>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
