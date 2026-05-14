using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Easrms.Application.Mappings;

namespace Easrms.Application
{
    public static class DepandancyInjection 
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DepandancyInjection).Assembly;
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper with the application's mapping profile
            services.AddAutoMapper(cfg => { }, assembly);

            // Register application services, handlers, etc. here
            // e.g. services.AddScoped<IRequestHandler<CreateRequestCommand>, CreateRequestCommandHandler>();
            return services;
        }
    }
}
