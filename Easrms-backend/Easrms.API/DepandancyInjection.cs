using Easrms.Application;
using Easrms.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Easrms.API
{
    public static class DepandancyInjection 
    {
        public static IServiceCollection AddApi(this IServiceCollection services , IConfiguration configuration)
        {
            services.AddApplication().AddInfra(configuration);
            
            return services;
        }
    }
}
