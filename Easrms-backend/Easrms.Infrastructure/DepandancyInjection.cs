using Easrms.Application.Interfaces;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Infrastructure.BackgroundWorkers.Workers;
using Easrms.Infrastructure.Data;
using Easrms.Infrastructure.Export;
using Easrms.Infrastructure.Repositories.Implementations;
using Easrms.Infrastructure.Services;
using Easrms.Infrastructure.Services.Email;
using Easrms.Infrastructure.Services.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Easrms.Infrastructure
{
    public static class DepandancyInjection 
    {

        public static IServiceCollection AddInfra(
     this IServiceCollection services,
     IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IEscalationRepository, EscalationRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthProviderRepository, AuthProviderRepository>();
            services.AddScoped<IOAuthService, GoogleOAuthService>(); // default registration for IOAuthService is Google; specific services registered below
            services.AddScoped<IOAuthService, GoogleOAuthService>();
            services.AddScoped<IOAuthService, GitHubOAuthService>();
            services.AddScoped<IOAuthService, AzureOAuthService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddHostedService<RetryFailedEmailWorker>();
            services.AddHostedService<ExpiredRefreshTokenCleanupWorker>();
            // Register Cloudinary service and bind settings
            services.Configure<Easrms.Application.Settings.CloudinarySettings>(configuration.GetSection("Cloudinary"));
            services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Easrms.Application.Settings.CloudinarySettings>>().Value as Easrms.Application.Interfaces.ICloudinarySettings);
            services.AddSingleton<IEmailQueue, EmailQueue>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            services.AddScoped<ExcelExportService>();
            services.AddScoped<PdfExportService>();

            services.AddScoped<DapperContext>();
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            //Console.WriteLine("String "+connectionString);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            return services;
        }
    }
}
