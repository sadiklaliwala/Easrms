using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Easrms.Application.Mappings;
using Easrms.Application.Settings;
using Easrms.Application.Interfaces.Jwt;
using MediatR;
using Easrms.Application.Behaviors;

namespace Easrms.Application
{
    public static class DepandancyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddScoped<IJwtSettings, JwtSettings>();
            var assembly = typeof(DepandancyInjection).Assembly;
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper with the application's mapping profile
            services.AddAutoMapper(cfg => { }, assembly);

            // Register pipeline behavior to log exceptions from handlers
            services.AddTransient(typeof(IPipelineBehavior<,>),typeof(ExceptionLoggingBehavior<,>)
);

            // Register Intent Handlers dynamically
            var intentHandlerType = typeof(Easrms.Application.Features.ChatMessage.Intents.IIntentHandler);
            var intentHandlers = assembly.GetTypes()
                .Where(t => intentHandlerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var handler in intentHandlers)
            {
                services.AddScoped(intentHandlerType, handler);
            }

            return services;
        }
    }
}
