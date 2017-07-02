using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Nubizsoft.AspNetCore.Authentication.AzureADB2C
{
    public static class ServiceExtensions
    {
		public static IServiceCollection AddAzureADB2C(this IServiceCollection services, IConfigurationSection configure)
		{
            services.Configure<AzureAdB2COptions>(configure);

            return services.AddInternalAzureADB2C();
		}

		public static IServiceCollection AddAzureADB2C(this IServiceCollection services, Action<AzureAdB2COptions> options)
		{
			services.Configure<AzureAdB2COptions>(options);

			return services.AddInternalAzureADB2C();
		}

		internal static IServiceCollection AddInternalAzureADB2C(this IServiceCollection services)
        {
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, AzureAdB2COpenIdConnectOptionsSetup>();

			return services;
        }
    }
}
