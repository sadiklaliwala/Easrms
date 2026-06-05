using Easrms.API;
using Easrms.API.Middleware;
using Easrms.Application.Interfaces.Cloudinary;
using Easrms.Application.Interfaces.Jwt;
using Easrms.Application.Settings;
using Easrms.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    //.ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddControllers();

builder.Services.AddApi(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// Bind JwtSettings and expose as IJwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Expose the bound JwtSettings instance as IJwtSettings for consumers that take IJwtSettings
builder.Services.AddSingleton<IJwtSettings>(sp =>
    sp.GetRequiredService<IOptions<JwtSettings>>().Value);

// Bind and expose Cloudinary settings as ICloudinarySettings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddSingleton<ICloudinarySettings>(sp =>
    sp.GetRequiredService<IOptions<CloudinarySettings>>().Value);

//builder.Services.AddAutoMapper(
//    typeof(Easrms.Application.Mappings.MappingProfile).Assembly);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
Console.WriteLine("Jwtsection "+builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey!))
    };

    // Read JWT from HttpOnly Cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies[builder.Configuration["JwtSettings:CookieName"]!];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = new[] {
        "https://easrms-git-dev-sadiks-projects-e4d0ed68.vercel.app"
    };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
// Console.WriteLine(builder.Environment.EnvironmentName);
var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.MapGet("/", (context) =>
{

    return context.Response.WriteAsync("Service is Running  and Allowes Origin is "+allowedOrigins);
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Map SignalR notification hub
app.MapHub<Easrms.API.Hubs.NotificationHub>("/hubs/notification");

app.Run();

