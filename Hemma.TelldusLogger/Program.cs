using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Hemma.TelldusLogger
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            // Kick off our actual code
            await serviceProvider.GetService<ITelldusLogger>().Run();

        }

        private static IServiceCollection ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddTransient<ITelldusLogger, TelldusLogger>();
            services.AddTransient<IDataFetcher, DataFetcher>();
            services.AddTransient<ITelldusTemperatureFactory, TelldusTemperatureFactory>();
            services.AddTransient<ITelldusRepository, TelldusRepository>();
            services.AddHttpClient();

            return services;
        }
    }
}
