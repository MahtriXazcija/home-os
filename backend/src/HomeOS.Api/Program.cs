using System.Text;
using System.Text.Json;
using HomeOS.Application.Households;
using HomeOS.Application.Households.Commands;
using HomeOS.Infrastructure.Identity;
using HomeOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateHouseholdCommand>());

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

    builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();

    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

    var jwtSecret = builder.Configuration[$"{JwtOptions.SectionName}:Secret"];
    if (!string.IsNullOrWhiteSpace(jwtSecret))
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
                };
            });
        builder.Services.AddAuthorization();
    }
}

var app = builder.Build();

// Placed first so it wraps every downstream middleware, including CORS —
// otherwise an unhandled exception aborts the connection before CORS
// headers are attached, and the browser reports a misleading "Failed to
// fetch" instead of the real error.
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = ex.GetType().Name,
            message = ex.Message
        }));
    }
});

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

if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(builder.Configuration[$"{JwtOptions.SectionName}:Secret"]))
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
