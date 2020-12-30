using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Foundry.Web.Grains.GuildBuilder
{
    public sealed class GuildGrain : Grain, IGuild
    {
        private readonly ILogger Logger;

        public GuildGrain(ILogger<GuildGrain> logger)
        {
            Logger = logger;
        }

        public Task<string> CreateMemberApplication(string applicationData)
        {
            Logger.LogInformation("Creating member application for guild {GuildId} - {Data}", this.GetPrimaryKey().ToString(), applicationData);
            return Task.FromResult($"Creating member application for guild {this.GetPrimaryKey()} - {applicationData}");
        }
    }
}
