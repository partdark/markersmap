using Application.Services;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DI
{
    public static class DI
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructureServices(configuration);
            
            // Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IMarkerService, MarkerService>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<IAuthService, AuthService>();
            
            return services;
        }
    }
}
