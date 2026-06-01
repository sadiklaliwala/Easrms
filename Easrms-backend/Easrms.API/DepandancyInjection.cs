using Easrms.Application;
using Easrms.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Easrms.API.Services;
using Easrms.Application.Interfaces.Notifications;

namespace Easrms.API
{
    public static class DepandancyInjection 
    {
        public static IServiceCollection AddApi(this IServiceCollection services , IConfiguration configuration)
        {
            services.AddApplication().AddInfra(configuration);
            // Register SignalR hubs
            services.AddSignalR();

            // Register notification publisher implementation
            services.AddScoped<INotificationPublisher, NotificationPublisher>();

            return services;
        }
    }
}
