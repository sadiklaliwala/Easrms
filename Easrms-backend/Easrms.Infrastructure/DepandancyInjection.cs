using Easrms.Application.Interfaces.Repositories;
using Easrms.Infrastructure.Data;
using Easrms.Infrastructure.Repositories.Implementations;
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
