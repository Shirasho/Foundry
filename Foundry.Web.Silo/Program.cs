using System;
using System.Threading.Tasks;
using Foundry.Web.Grains.GuildBuilder;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Foundry.Web.Silo
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                await using var host = CreateHost();
                await host.StartAsync();

                Console.WriteLine("\n\nPress any key to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        private static ISiloHost CreateHost()
        {
            return new SiloHostBuilder()
#if DEBUG
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "FoundryWeb";
                })
#endif
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(GuildGrain).Assembly).WithReferences();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();
        }
    }
}
