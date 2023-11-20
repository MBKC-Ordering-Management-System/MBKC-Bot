using MBKC.Repository.Infrastructures;
using MBKC.Service.Services.Implementations;
using MBKC.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.WokerService.Extentions
{
    public static class DependencyExtention
    {
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddSingleton<IUnitOfWork, UnitOfWork>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IStoreService, StoreService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<IRabbitMQService, RabbitMQService>();
            services.AddSingleton<IUserDeviceService, UserDeviceService>();
            return services;
        }
    }
}
